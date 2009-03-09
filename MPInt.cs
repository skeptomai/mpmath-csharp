using System;
using System.Collections;
using System.Text;
using System.Diagnostics;

namespace MPMath {
	public enum MPSizes {
		NormalSize = 500,
		DoubleSize = 1000,
		Base       = 65536,
		ExpBase    = 32
	}
  
	public enum DisplayBases {
		Octal = 8,
		Hex   = 16,
		Dec   = 10
	}


	/// <summary>
	/// Summary description for MPInt.
	/// </summary>
	struct MPInt : IEnumerable, IComparable, ICloneable {
    
		// Members
		UInt16[]         digitBlock;
		int              numDigits;
		int              capacity;
		DisplayBases     displayBase;
		static private char[] displayDigits = {'0','1','2','3','4','5','6','7','8','9','a','b','c','d','e','f'};
		static private UInt16[] bitmasks = {0,2,4,8,16,32,64,128,256,512,1024,2048,4096,8192,16384,32768};

		private MPInt(UInt16[] digitBlock, int numDigits) {
			this.digitBlock = digitBlock;
			this.numDigits = numDigits;
			capacity = (int)MPSizes.NormalSize;
			this.displayBase = DisplayBases.Dec;
		}

		#region operators    
		/// <summary>
		/// This is value equality
		/// </summary>
		public static bool operator ==(MPInt q1, MPInt q2) 
		{
			return (0==q1.CompareTo(q2));
		}
    
		/// <summary>
		/// This is value equality opposite
		/// </summary>
		public static bool operator !=(MPInt q1, MPInt q2) {
			return (0!=q1.CompareTo(q2));
		}
    
		/// <summary>
		/// operator less than
		/// </summary>
		public static bool operator <(MPInt q1, MPInt q2) {
			return (q1.CompareTo(q2) < 0);
		}

		/// <summary>
		/// operator greater than
		/// </summary>
		public static bool operator >(MPInt q1, MPInt q2) {
			return (q1.CompareTo(q2) > 0);
		}

		/// <summary>
		/// operator greater than or equal to
		/// </summary>
		public static bool operator >=(MPInt q1, MPInt q2) {
			return (q1.CompareTo(q2) >=0);
		}

		/// <summary>
		/// operator less than or equal to
		/// </summary>
		public static bool operator <=(MPInt q1, MPInt q2) {
			return (q1.CompareTo(q2) <=0);
		}
		
		/// <summary>
		/// addition operator between two MPInt's
		/// </summary>
		public static MPInt operator +(MPInt summand1, MPInt summand2) {
			return summand1.Add(summand2);
		}

		/// <summary>
		/// addition operator, MPInt and long
		/// </summary>
		public static MPInt operator +(MPInt summand1, long summand2) {
			return summand1.Add(summand2);
		}

		/// <summary>
		/// subtraction operator, MPInt's
		/// </summary>
		public static MPInt operator -(MPInt minuend, MPInt subtrahend) {
			return minuend.Sub(subtrahend);
		}

		/// <summary>
		/// subtraction operator MPInt minus long
		/// </summary>
		public static MPInt operator -(MPInt minuend, long subtrahend) {
			return minuend.Sub(subtrahend);
		}

		/// <summary>
		/// division operator, MPInt's
		/// </summary>
		public static MPInt operator /(MPInt dividend, MPInt divisor) {
			return dividend.Div(divisor);
		}

		/// <summary>
		/// division operator, MPInt and long
		/// </summary>
		public static MPInt operator /(MPInt dividend, long divisor) {
			return dividend.Div(divisor);
		}

		/// <summary>
		/// multiplication operator, MPInt's
		/// </summary>
		public static MPInt operator *(MPInt factor1, MPInt factor2) {
			return factor1.Mult(factor2);
		}

		/// <summary>
		/// multiplication operator, MPInt and long
		/// </summary>
		public static MPInt operator *(MPInt factor1, long factor2) {
			return factor1.Mult(factor2);
		}

		/// <summary>
		/// bitwise asl
		/// </summary>
		public static MPInt operator <<(MPInt shifter, int shifts) {
			MPInt shifted = (MPInt)shifter.Clone();
			shifted.ASL(shifts);
			return shifted;
		}

		/// <summary>
		/// bitwise asr
		/// </summary>
		public static MPInt operator >>(MPInt shifter, int shifts) {
			MPInt shifted = (MPInt)shifter.Clone();
			shifted.ASR(shifts);
			return shifted;
		}

		/// <summary>
		/// incr operator
		/// </summary>
		public static MPInt operator++(MPInt incr) {
			return incr+=1;
		}

		/// <summary>
		/// decr operator
		/// </summary>
		public static MPInt operator--(MPInt decr) {
			return decr-=1;
		}

		/// <summary>
		/// cast from long
		/// </summary>
		public static implicit operator MPInt(long l) {
			if(l<0)
				throw new ArgumentOutOfRangeException();

			MPInt mpret = new MPInt();
			for(int index=0; index<4; index++) {
				mpret[index]=(int)(l&0xffff);
				l >>= 16;
			}

			mpret.Trim();
			return mpret;
		}

		/// <summary>
		/// cast from int
		/// </summary>
		public static implicit operator MPInt(int i) {
			MPInt mpret = (long)i;
			return mpret;
		}

		#endregion

		#region object_overrides
		/// <summary>
		/// This is reference equality
		/// </summary>
		public override bool Equals(Object obj) 
		{
			return obj is MPInt && (this==(MPInt)obj);
		}

		// Object override - BUGBUG
		public override int GetHashCode() {
			return 1;
		}

		///<summary>
		/// override of ToString from object
		///</summary>
		public override string ToString() 
		{
			return DigitsByBase();
		}

		#endregion

		#region ICloneable
		/// <summary>
		/// implementation of ICloneable
		/// </summary>
		public object Clone() 
		{
			MPInt mpclone = (MPInt)this.MemberwiseClone();
			mpclone.digitBlock = new UInt16[mpclone.capacity];
			Trim();
			Array.Copy(DigitBlock,0,mpclone.DigitBlock,0,DigitBlock.Length);
			mpclone.displayBase = this.displayBase;

			return mpclone;
		}
		#endregion

		#region IEnumerable
		/// <summary>
		///
		/// </summary>
		public IEnumerator GetEnumerator() 
		{
			return null;
		}
		#endregion
		
		#region IComparable
		/// <summary>
		/// implementation of IComparable
		/// </summary>
		public int CompareTo(object objCompareTo) 
		{
			MPInt mpu1 = (MPInt)objCompareTo;
			int mydigits = numDigits;
			int otherdigits = mpu1.numDigits;

			// strip leading zeros from source and dest in
			// comparison
			while(mydigits >0 && this[mydigits-1]==0)
				mydigits--;
			while(otherdigits >0 && mpu1[otherdigits-1]==0)
				otherdigits--;

			if( (mydigits == otherdigits) && (otherdigits==0)) 
			{
				return 0;
			}
			else 
			{
				// often we can just compare number of digits
				if(mydigits>otherdigits)
					return 1;
				if(otherdigits>mydigits)
					return -1;

				// crap, same number of digits... do real work
				// it would be cooler to fix this memory down and
				// memcmp it
#if ITER
				int i=mydigits-1;
				while(i>=0) 
				{
					if(this[i]!=mpu1[i])
						return (this[i]>mpu1[i] ? 1 : -1);
					i--;
				}
#else
				fixed(UInt16* pfmd = this.DigitBlock, pfmpu1 = mpu1.DigitBlock)
				{
					UInt16* pmd = pfmd+mydigits-1; UInt16* pmpu1 = pfmpu1+mydigits-1;																	  
					for(int i=mydigits-1; i>=0; i--)
					{
						if(*pmd != *pmpu1)
							return (*pmd > *pmpu1 ? 1 : -1);
						pmd--;
						pmpu1--;
					}
				}
			}
#endif
			}
			return 0;
		}		

		#endregion
		
		/// <summary>
		/// DisplayBase - sets display in Hex,Dec, Binary
		/// </summary>
		public DisplayBases DisplayBase 
		{
			get { return displayBase; }
			set { displayBase = value; }
		}

		/// <summary>
		///
		/// </summary>
		public int Length {
			get {
				return numDigits;
			}
		}
		
		/// <summary>
		///
		/// </summary>
		public int Capacity {
			get {
				return DigitBlock.Length;
			}
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="factor1"></param>
		/// <returns></returns>
		public static MPInt SlowSqr(MPInt factor1) {
			return factor1*factor1;
		}

		public static MPInt Sqr(MPInt factor1) {
			UInt32 tempResult=0;
			UInt16 carry=0;
			UInt16[] p1=new UInt16[(int)MPSizes.DoubleSize];
			UInt16[] factor = factor1.DigitBlock;
			int n = factor1.Length;

			for(int i=0; i<=n-2; i++) {
				carry=0;
				for(int j=i+1; j<=n-1; j++) {
					tempResult = p1[i+j]+(UInt32)((factor[i]*factor[j]))+carry;
					p1[i+j]=(UInt16)(tempResult &  0xffff);
					carry=(UInt16)(tempResult>>16);
				}
				p1[i+n]=(UInt16)carry;
			}

			carry=0;
			for(int i=0; i<= 2*n-2; i++) {
				tempResult = (UInt32)(2*p1[i]+carry);
				p1[i]=(UInt16)(tempResult & 0xffff);
				carry=(UInt16)(tempResult>>16);
			}

			carry=0;
			for(int i=0; i<=n-1; i++) {
				tempResult = p1[2*i] + (UInt32)(factor[i]*factor[i]) + carry;
				p1[2*i]=(UInt16)(tempResult & 0xffff);
				carry = (UInt16)(tempResult >> 16);
				tempResult = (UInt32)(p1[2*i+1] + carry);
				p1[2*i+1]=(UInt16)(tempResult & 0xffff);
				carry=(UInt16)(tempResult >> 16);
			}


			int digits = 2*n;
			MPInt retval = new MPInt();
			Array.Copy(p1,0,retval.DigitBlock,0,digits);
			retval.SetLength(digits);
			retval.DisplayBase = factor1.DisplayBase;
			return retval;

		}

		public static MPInt Mod2(MPInt val, int modpow) {
      
			int modpowdigits = (modpow / 16);
			int rembits = modpow - ((modpow / 16)* 16);
      
			// if the mod digit > what we have
			// our entire value fits in the remainder
			if(modpowdigits > val.Length)
				return (MPInt)val.Clone();
      
			UInt16 lastdig = (UInt16)((0xffff >> (16 - rembits)) & val[modpowdigits]);
      
			MPInt modded = new MPInt();
			modded.DisplayBase = val.DisplayBase;
			Array.Copy(val.DigitBlock,0,modded.DigitBlock,0,modpowdigits);
			modded[modpowdigits]=lastdig;
      
			return modded;
      
		}
    
		public static MPInt Mod(MPInt val, MPInt mpmod) {
      
			int modpowdigits = mpmod.Length;
			int rembits = BitCount((UInt16)mpmod[val.Length-1]);
      
			// if the mod digit > what we have
			// our entire value fits in the remainder
			if(mpmod > val)
				return (MPInt)val.Clone();
      
			return val - (val / mpmod) * mpmod;
		}
    
		public static MPInt ModAdd(MPInt add1, MPInt add2, MPInt mod) {
			if(mod.Length==0) {
				throw new ArgumentOutOfRangeException();
			}
      
			MPInt sum=add1+add2;
			return MPInt.Mod(sum,mod);
		}
		public static MPInt ModSub(MPInt minuend, MPInt subtrahend, MPInt mod) {
			if(mod.Length==0) {
				throw new ArgumentOutOfRangeException();
			}
      
			MPInt difference=minuend-subtrahend;
			return MPInt.Mod(difference,mod);
		}
		public static MPInt ModMult(MPInt factor1, MPInt factor2, MPInt mod) {
			if(mod.Length==0) {
				throw new ArgumentOutOfRangeException();
			}
      
			MPInt product=factor1*factor2;
			return MPInt.Mod(product,mod);
		}
		public static MPInt ModSqr(MPInt factor1, MPInt mod) {
			if(mod.Length==0) {
				throw new ArgumentOutOfRangeException();
			}
      
			return MPInt.Mod(MPInt.Sqr(factor1),mod);
		}
		public static MPInt Mod2Sqr(MPInt factor1, int modpow) {
			if(modpow<0)
				throw new ArgumentOutOfRangeException();
			return MPInt.Mod(MPInt.Sqr(factor1),modpow);
		}
		public static bool  ModEquiv(MPInt minuend, MPInt subtrahend, MPInt mod) {
			if(mod.Length==0) {
				throw new ArgumentOutOfRangeException();
			}
      
			MPInt difference=minuend-subtrahend;
			difference.Trim();
			return (MPInt.Mod(difference,mod).Length==0) ;
		}
		public static MPInt ModExp(MPInt factor, MPInt mod, int exp) {
			if(exp<0 || mod<=0)
				throw new ArgumentOutOfRangeException();
      
			MPInt retval = (MPInt)factor.Clone();
			retval = Mod(retval, mod);
			int mask = (int)MPSizes.Base/2;
			while((mask & exp)==0) {
				mask >>=1;
			}
			mask >>=1;
      
			while(mask>0) {
				retval = ModSqr(retval,mod);
				if((exp & mask)!=0) {
					retval = ModMult(retval,factor,mod);
				}
				mask>>=1;
			}
      
			return retval;
		}
		public static MPInt Mod5Exp(MPInt factor, MPInt mod, MPInt exp) {
			// precompute factor table
			MPInt factsq = factor * factor;
			MPInt[] oddPow = new MPInt[(int)MPSizes.ExpBase/2]; 
			oddPow[0] = (MPInt)factor.Clone();
			// we need one extra exponent digit
			// to compute the base k digits - we may spill one
			// digit off the end
			MPInt expInt = (MPInt)exp.Clone();
			expInt[expInt.Length]=0;
			for(int n=1; n<(int)MPSizes.ExpBase/2; n++) {
				oddPow[n]=factsq*oddPow[n-1];
			}
      
			// we need the number of binary digits
			// an int type can easily hold this
			int binaryDigits = exp.NumBinaryDigits;
			return new MPInt();
      
		}
		public int NumBinaryDigits {
			get {
				// caution, bigint should not be carrying
				// leading zeroes into this function!
				// Length gives number of B base digits - 
				// we need binary digits which is 16* (exp.Length-1) + number of bits in exp.Length[-1]
				if(this[Length-1]==0)
					throw new ArithmeticException("argument cannot contain leading zeros");
	
				uint mask = 0x8000;
				int topDigit = this[Length-1];
				int bits = 16;
				while((topDigit & mask)==0 && mask>0) {
					mask >>= 1;
					bits--;
				}
				return bits+(16*(this.Length-1));
			}
		}
		public int Num5Digits {
			get {
				return (int)Math.Floor( (NumBinaryDigits-1)/5 );
			}
		}
		public int Exp5Dig(int index) {
			//digit calc
			int k = 32; // 2^5 for exp base
			int digitIndex = 0;
			int si = (int)Math.Floor((k * digitIndex)/16);
			int di = (k * digitIndex) % 16;
			return (int)(( ((ulong)this[si+1]<< 16) + (ulong)this[si]) >> di) & (31);
		}
		private UInt16[] DigitBlock {
			get {
				if(null == digitBlock){
					digitBlock = new UInt16[(int)MPSizes.NormalSize];
					capacity = (int)MPSizes.NormalSize;
					displayBase = DisplayBases.Dec;
					numDigits = 0;
				}
				return digitBlock;
			}
		}
    
		private int this[int index] {
			get {
				if(index<0 || index>=Capacity)
					throw new ArgumentOutOfRangeException();
				return DigitBlock[index];
			}
			set {
				if(index>Capacity-1 || index<0 || value < 0 || value >= (long)MPSizes.Base)
					throw new ArgumentOutOfRangeException();
				DigitBlock[index] = (ushort)value;
				if(index>=numDigits)
					numDigits=index+1;
			}
		}
    
		private void SetLength(int length) {
			if(length<0 || length>Capacity)
				throw new ArgumentOutOfRangeException();
			numDigits = length;				
			DigitBlock[numDigits]=0;
      
		}
    
		private MPInt Add(MPInt summand) {
			UInt16[] sumDigits = new UInt16[(int)MPSizes.NormalSize+1];
			UInt16[] summand1 = this.DigitBlock;
			UInt16[] summand2 = summand.DigitBlock;
      
			int maxdigits = Math.Max(this.Length,summand.Length)+1;
      
			System.UInt16 carry=0;
      
			// The result may be 1 greater than
			// capacity because of carry, this is overflow
			int i=0;
			for(i=0; i<maxdigits; i++) {
				long digsum = (summand1[i]+summand2[i]+carry);
				carry = (UInt16)(digsum >> 16);
				sumDigits[i] = (UInt16)(digsum & 0xffff);
			}
      
			if(carry!=0) {
				sumDigits[i++] = (ushort)carry;
			}
      
      
			// overflow
			if(i>=Capacity)
				throw new ArithmeticException();
      
			MPInt sum = new MPInt();
			sum.DisplayBase = this.DisplayBase;
			Array.Copy(sumDigits,0,sum.DigitBlock,0,i);
			sum.SetLength(i);
      
			return sum;
		}
    
		private MPInt Add(long summand) {
      
			if(summand < 0 || summand > (long)MPSizes.Base)
				throw new ArgumentOutOfRangeException();
      
			UInt16[] tempSum = new UInt16[(int)MPSizes.NormalSize+1];
      
			long carry=summand;
      
			// The result may be 1 greater than
			// capacity because of carry, this is overflow
			int i=0;
			for(i=0; i<Length; i++) {
				long digsum = (this[i]+carry);
				carry = (digsum >> 16);
				tempSum[i] = (UInt16)(digsum & 0xffff);
			}
      
			if(carry!=0) {
				tempSum[i++]=(ushort)carry;
			}
      
			// overflow
			if(i>=Capacity)
				throw new ArithmeticException();
      
			MPInt sum = new MPInt();
			sum.DisplayBase = this.DisplayBase;
			Array.Copy(tempSum,0,sum.DigitBlock,0,sum.DigitBlock.Length);
			sum.SetLength(i);
			return sum;
		}
    
		private MPInt Sub(MPInt subtrahend) {
			MPInt retval;
      
			// subtrahend >= minuend
			int compares = CompareTo(subtrahend);
      
			if(compares<0)
				throw new ArithmeticException();			
      
			UInt16[] tempResult = new UInt16[(int)MPSizes.NormalSize];
      
			if(compares == 0) {
				retval = new MPInt(tempResult,numDigits);
				retval.DisplayBase = this.DisplayBase;
				retval.numDigits = numDigits;
				return retval;
			}
      
			int carry=0;
			int i=0;
			for(i=0; i<Length; i++) {
				int resdigit = this[i]-subtrahend[i]+carry;
				if(resdigit<0) {
					resdigit+=(int)MPSizes.Base;
					carry=-1;
				}
				else
					carry=0;
				tempResult[i]=(UInt16)resdigit;
			}
      
			// underflow exception
			if(carry==-1)
				throw new ArithmeticException();
      
			retval = new MPInt(tempResult, i);
			retval.DisplayBase = this.DisplayBase;
			return retval;
      
		}
    
		private MPInt Sub(long subtrahenddigit) {
			MPInt retval;
      
			if(subtrahenddigit<0 || subtrahenddigit>(int)MPSizes.Base)
				throw new ArgumentOutOfRangeException();
      
			MPInt subtrahend = new MPInt();
			subtrahend.DisplayBase = this.DisplayBase;
			subtrahend[0] = (ushort)subtrahenddigit;
      
			// subtrahend >= minuend
			int compares = CompareTo(subtrahend);
      
			if(compares<0)
				throw new ArithmeticException();			
      
			UInt16[] tempResult = new UInt16[(int)MPSizes.NormalSize];
      
			if(compares == 0) {
				retval = new MPInt(tempResult,numDigits);
				retval.DisplayBase = this.DisplayBase;
				retval.numDigits = numDigits;
				return retval;
			}
      
			int carry=0;
			int i=0;
			for(i=0; i<Length; i++) {
				int resdigit = this[i]-subtrahend[i]+carry;
				if(resdigit<0) {
					resdigit+=(int)MPSizes.Base;
					carry=-1;
				}
				else
					carry=0;
				tempResult[i]=(UInt16)resdigit;
			}
      
			// underflow exception
			if(carry==-1)
				throw new ArithmeticException();
      
			retval = new MPInt(tempResult, i);
			retval.DisplayBase = this.DisplayBase;
			return retval;
      
		}
    
		private MPInt Mult(long m1) {
      
			if(m1 <0 || m1 >(long)MPSizes.Base)
				throw new ArgumentOutOfRangeException();
      
			UInt16[] tempResult = new UInt16[(int)MPSizes.NormalSize];
      
			System.UInt16 carry = 0;
			System.UInt32 um2 = 0;
			int i=0;
      
			for(i=0; i < Length; i++) {
				um2 = (UInt32)(this[i]*m1)+carry;
				carry=(UInt16)(um2>>16);
				tempResult[i]=(System.UInt16)(um2 & 0xffff);
			}
      
			if(carry!=0) {
				if(i==Capacity)
					throw new ArithmeticException();
				tempResult[i++]=carry;
			}
      
			MPInt retval = new MPInt(tempResult,i);
			retval.DisplayBase = this.DisplayBase;
			return retval;
		}
    
		private MPInt Mult(MPInt factor2) {
			UInt16[] productdigits = new UInt16[(int)MPSizes.NormalSize*2];

			int carry=0;

			UInt16[] factor1dig = this.DigitBlock;
			UInt16[] factor2dig = factor2.DigitBlock;
#if ITER
			int i=0;
			foreach(UInt16 ui in factor1dig){
				int j=0;
				foreach(UInt16 vj in factor2dig){
					System.UInt32 up = (UInt32)(ui*vj+carry+productdigits[i+j]);
					productdigits[i+j] = (UInt16)(up & 0xffff);
					carry=(UInt16)(up>>16);
					j++;
				}
				i++;
			}
#else
			fixed(UInt16* pfui = factor1dig, pfvj = factor2dig, pfpd = productdigits)
			{
				UInt16* pui = pfui;
				UInt16* pvj = pfvj;
				UInt16* ppd = pfpd;
				for(int if1=0; if1<factor1dig.Length; if1++)
				{
					for(int jf1=0; jf1<factor2dig.Length; jf1++)
					{
						System.UInt32 up = (UInt32)( *(pui+if1) * *(pvj+jf1) + carry + (*(ppd + if1+ jf1)));
						*(ppd + if1 + jf1) = (UInt16)(up & 0xffff);
						carry=(UInt16)(up>>16);
					}
				}
			}
#endif      
			int ndig = this.Length+factor2.Length+1;

			// overflow
			if(ndig > (int)MPSizes.NormalSize)
				throw new ArithmeticException();

			MPInt retval = new MPInt();
			Array.Copy(productdigits,0,retval.DigitBlock,0,ndig);
			retval.SetLength(ndig);
			retval.DisplayBase = this.DisplayBase;
			return retval;
		}				
    
		private MPInt Div(MPInt divisor) {
      
			MPInt u = (MPInt)Clone();
			MPInt v = (MPInt)divisor.Clone();
			u.Trim();
			v.Trim();
			MPInt q  = new MPInt();
      
			// Can't divide by zero
			if(v.numDigits == 0)
				throw new ArgumentException();
      
			// dividing zero by something else equals zero
			if(u.numDigits == 0) {
				return q;
			}
      
			// if the v only has one digit, use the simple routine
			if(v.numDigits == 1) {
				return u / v[0];
			}
      
			// ok, long route
			// normalize to give better qhat estimates
			// this raises number of digits in u by one
			// (top digit may be zero) and does not raise the number
			// of digits in v (since we've just scaled its top
			// digit to be between (int)MPSizes.Base/2 and (int)MPSizes.Base
			long scale = 1;
      
			int n = v.Length;
			int m = u.Length-n;
      
			int v_msd = v[n-1];
      
			// scale up v
			while(v_msd < ((int)MPSizes.Base)/2) {
				v_msd <<= 1;
				scale <<= 1;
			}
      
			// if no shift occurs, or if the multiplication
			// doesn't cause a carry into a higher digit
			// we will add an additional 0 digit anyway
			int u_inc_digits = u.Length+1;
      
			if(scale != 1) {
				// This may or may not increment the number of digits in u... 
				// must check this
				int digits = u.Length;
				u *= scale;
				v *= scale;
			}
      
			u.SetLength(u_inc_digits);
      
			// initialize j
			for(int j=m; j>=0; j--) {
				// generate qhat
				// From Knuth (Uj+nB + Uj+n-1)/(Vn-1)
				long uhat = (((long)u[j+n]) << 16) + ((long)u[j+n-1]) ;
				long vhat = (long)v[n-1];
				long qhat = uhat  / vhat ;
				long rhat = uhat - (qhat * vhat);
	
				long test1 = qhat*v[n-2];
				long test2 = ((int)MPSizes.Base * rhat) + ( (j+n-2) >=0 ? (int)u[j+n-2] : (int)0);
	
				// Make sure we didn't overflow in
				// creating the test values
				Debug.Assert(test1>=0 && test2>=0);
	
				// decrease qhat by one if it is (int)MPSizes.Base or test fails
				if(qhat == (int)MPSizes.Base || test1 > test2) {
					qhat--;
					rhat += v[n-1];
					test1 = qhat*v[n-2];
					test2 = ((int)MPSizes.Base * rhat) + ( (j+n-2) >=0 ? (int)u[j+n-2] : (int)0);
	  
					// qhat is still 1 too great
					if(rhat < (int)MPSizes.Base && (qhat == (int)MPSizes.Base || test1>test2)) {
						qhat--;
					}
	  
				}
	
				Debug.Assert(qhat < (int)MPSizes.Base && qhat>=0 && rhat>=0);
	
				// Multiply and subtract
				// subtract term from top term.Length digits of u
				// easiest done as a shift of term?
	
				MPInt term = v * qhat;
				term <<= j ;
	
				// if the result would be negative, then
				// we oopsd again
				if(u < term) {
					qhat--;
					term = v *qhat;
					term <<= j;
				}
	
				u -= term;
	
				// set quotient digit
				q[j]=(ushort)qhat;
	
			}
      
			q.Trim();
			q.DisplayBase = this.DisplayBase;
			return q;
		}
    
		private MPInt Div(long i) {
			if(i < 0 || i > (int)MPSizes.Base)
				throw new ArgumentOutOfRangeException();
      
			MPInt pseudodividend = (MPInt)this.Clone();
			pseudodividend <<= 1;
			MPInt divisor = new MPInt();
			divisor.DisplayBase = this.DisplayBase;
			divisor[1]=(int)i;
			return pseudodividend / divisor;
		}

		private void Trim() { 
			while(numDigits>0 && this[numDigits-1]==0)
				numDigits--;
		}
		
		private void ASL(int shift) {
			if(Length>0) {
				UInt16[] newArray = new UInt16[Capacity];
				Array.Copy(digitBlock,0,newArray,shift,this.Length);
				digitBlock = newArray;
				SetLength(Length+shift);
				Trim();
			}
		}

		private void ASR(int shift) {
			if(Length>0) {
				UInt16[] newArray = new UInt16[Capacity];

				if(shift>=Length) {
					SetLength(0);
					digitBlock = newArray;
				}
				else {
					int digitsLeft = Length - shift;
					Array.Copy(digitBlock,shift,newArray,0,digitsLeft);
					digitBlock = newArray;
					SetLength(digitsLeft);
				}
			}
		}
		
		private static int BitCount(UInt16 bitbag) {
			int retval=0;
			for(int i=0; i < 16; i++) {
				if( (bitbag & bitmasks[i]) >0) {
					retval ++;
				}
			}
			return retval;
		}


		private string DigitsByBase() {
			ArrayList cc = new ArrayList();
			MPInt tempUnit = (MPInt)this.Clone();
			
			while(tempUnit.Length>0) {
				MPInt q = tempUnit / (int)displayBase;
				MPInt r = tempUnit - (q * (int)displayBase);
				tempUnit = q;
				cc.Add(displayDigits[r[0]].ToString());
			}

			cc.Reverse();

			StringBuilder sb = new StringBuilder();
			foreach(string digstring in cc) {
				sb.Append(digstring);
			}

			return sb.ToString();
		}

	}
}
