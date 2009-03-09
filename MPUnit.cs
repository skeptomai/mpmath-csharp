using System;
using System.Text;
using System.Diagnostics;
using System.Collections;
using ExtendedCollections;

namespace MPMath
{
	public enum OutputFormatBase
	{
		Hex,
		Dec,
		Binary,
		Octal
	}

	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class MPUnit : IComparable
	{
		public static int NormalSize = 200;
		private int maxDigits = NormalSize;
		private int numDigits = 0;
		private System.UInt16[] digitBlocks;
		public const int BASE=65536; // 2^16;
		public const int BASEBITS=16;
		private OutputFormatBase outputFormatBase = OutputFormatBase.Dec;
		private char[] decdigits= {'0','1','2','3','4','5','6','7','8','9'};

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			Console.WriteLine("Simple addition test");

			MPUnit mpu1 = new MPUnit();
			mpu1 = MPUnit.Add(mpu1,(ushort)65535);

			MPUnit mpu2 = new MPUnit();
			mpu2 = MPUnit.Add(mpu2,1);

			Console.WriteLine("mpu1: {0}",mpu1);

			for(int i = 0; i<20; i++)
			{
				mpu1 = MPUnit.Add(mpu1,mpu2);
				Console.WriteLine("i: {0}, mpu1: {1}",i, mpu1);
			}

			for(int i=0; i<20; i++)
			{
				mpu1 = MPUnit.Sub(mpu1,mpu2);
				Console.WriteLine("i: {0}, mpu1: {1}",i, mpu1);
			}


			Console.WriteLine("Simple multiplication test");

			MPUnit mpu3 = new MPUnit();
			mpu3[1]=1;
			MPUnit mpu4 = new MPUnit();
			mpu4[0]=1;

			for(int i=0; i<10; i++)
				{
				mpu3 = MPUnit.Mult(mpu3,mpu4);
				mpu3.Trim();
				mpu4[0]= (ushort)(mpu4[0] + (ushort)1);
				Console.WriteLine("mpu3 {0} :: mpu4 {1}",mpu3, mpu4);
				Console.WriteLine("mpu3 digits {0}, mpu4 digits {1}",mpu3.Digits,mpu4.Digits);
				Console.WriteLine(mpu3.FormatDec());
				}


			Console.WriteLine("\nCompound division test");
			MPUnit dividend = new MPUnit();
			dividend[0]=15;
			dividend[1]=43202;
			dividend[2]=32720;
			dividend[3]=12;
			dividend[4]=4;
			dividend[5]=27;

			MPUnit divisor = new MPUnit();
			divisor[0]=65535;
			divisor[1]=16;

			MPUnit quotient=null,remainder=null;
			MPUnit.Div(dividend, divisor ,ref quotient, ref remainder);

			Console.WriteLine("Dividend {0}",dividend);
			Console.WriteLine("Divisor {0}",divisor);
			Console.WriteLine("Quotient {0}",quotient);
			Console.WriteLine(quotient.FormatDec());
			Console.WriteLine("Remainder {0}",remainder);
			MPUnit partialproduct = MPUnit.Mult(quotient,divisor);
			Console.WriteLine("Quotient * Divisor  = {0}",partialproduct);
			Console.WriteLine("Quotient * Divisor + Remainder = {0}",MPUnit.Add(partialproduct,remainder));

			Console.WriteLine("\nSimple division test");
			int littledivisor = 25;
			MPUnit.Div(dividend,littledivisor, ref quotient, ref remainder);
			
			Console.WriteLine("Dividend {0}",dividend);
			Console.WriteLine("little Divisor {0}",littledivisor);
			Console.WriteLine("Quotient {0}",quotient);
			Console.WriteLine("Remainder {0}",remainder);
			Console.WriteLine("Quotient * Divisor  = {0}",MPUnit.Mult(quotient,new MPUnit(littledivisor)));
			partialproduct = MPUnit.Mult(quotient,new MPUnit(littledivisor));
			Console.WriteLine("Quotient * Divisor  = {0}",partialproduct);
			Console.WriteLine("Quotient * Divisor + Remainder = {0}",MPUnit.Add(partialproduct,remainder));

			
		}

		public UInt16 this[int index]
		{
			get 
			{
				if(index <0 || index >= numDigits)
					throw new IndexOutOfRangeException();
				return digitBlocks[index];
			}
			set
			{
				if(index <0 || index >= maxDigits)
					throw new IndexOutOfRangeException();
				digitBlocks[index]=value;
				if(index>=numDigits)
					numDigits=index+1;
			}
		}

		public OutputFormatBase Base
		{
			get { return outputFormatBase; }
			set { outputFormatBase = value; }
		}

		public int Digits
		{
			get { return numDigits; }
			set { 
				if(value <0 || value >maxDigits)
					throw new ArgumentOutOfRangeException();
				numDigits = value; 
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public MPUnit()
		{
			digitBlocks = new System.UInt16[maxDigits];
		}

		public MPUnit(long i)
		{
			digitBlocks = new System.UInt16[maxDigits];
			if(i > BASE)
				throw new ArgumentOutOfRangeException();
			this[0] = (ushort)i;
		}

		/// <summary>
		/// MPUnit constructor that clones an MPUnit for
		/// use within math functions
		/// </summary>
		/// <param name="mpclone"></param>
		public MPUnit(MPUnit mpclone)
		{
			digitBlocks = new System.UInt16[mpclone.maxDigits];

			Array.Copy(mpclone.digitBlocks,0,this.digitBlocks,0,mpclone.digitBlocks.Length);

			numDigits = mpclone.numDigits;
			maxDigits = mpclone.maxDigits;

		}

		/// <summary>
		/// MPUnit constructor that creates 'capacity' digits
		/// </summary>
		/// <param name="capacity"></param>
		private MPUnit(int i)
		{
			digitBlocks = new System.UInt16[maxDigits];
			if(i > BASE)
				throw new ArgumentOutOfRangeException();
			this[0] = (ushort)i;
		}

		public void Clear()
		{
			digitBlocks = new System.UInt16[maxDigits];
		}


		public void ASL(int shift)
		{
			if(Digits>0)
			{
				UInt16[] newArray = new UInt16[maxDigits];
				Array.Copy(digitBlocks,0,newArray,shift,this.Digits);
				digitBlocks = newArray;
				this.Digits = this.Digits+shift;
			}
		}

		public void ASR(int shift)
		{
			if(Digits>0)
			{
				UInt16[] newArray = new UInt16[maxDigits];

				if(shift>=Digits)
				{
					Digits=0;
					digitBlocks = newArray;
				}
				else
				{
					int digitsLeft = Digits - shift;
					Array.Copy(digitBlocks,shift,newArray,0,digitsLeft);
					digitBlocks = newArray;
					Digits = digitsLeft;
				}
			}
		}

		public void Trim()
		{
			while(numDigits>0 && this[numDigits-1]==0)
				numDigits--;
		}

		/// <summary>
		/// Adds short to current MPUnit, returns sum in new MPUnit
		/// </summary>
		/// <param name="us1"></param>
		/// <returns></returns>
		public static MPUnit Add(MPUnit s1, System.UInt16 us1)
		{
			MPUnit sum = new MPUnit(s1);
			sum.Digits=s1.Digits+1;
			
			int i=0;
			System.UInt32 us2=0;
			System.UInt16 carry=us1;

			do
			{			
				us2 = (UInt32)(sum[i] + carry);
				carry = (System.UInt16)(us2 >> 16);
				sum[i] = (System.UInt16)(us2&0xffff);
				i++;
			}while(carry > 0 && i < sum.maxDigits);

			// overflow
			if(i == sum.maxDigits)
				throw new ArithmeticException();

			// Notice that, upon finishing the loop, i may be one greater
			// than the original sum.numDigits.  This shows a carry 
			// that propagated all the way through s1
			sum.Digits=Math.Max(i,s1.numDigits);

			return sum;
		}

		/// <summary>
		/// Adds an MPUnit to current MPUnit
		/// </summary>
		/// <param name="mps"></param>
		/// <returns>MPUnit</returns>
		public static MPUnit Add(MPUnit s1, MPUnit s2)
		{
			System.UInt16 carry=0;
			int maxdigits = Math.Max(s2.numDigits, s1.numDigits);

			MPUnit summand1 = new MPUnit(s1);
			MPUnit summand2 = new MPUnit(s2);
			summand1.Digits = maxdigits+1;
			summand2.Digits = maxdigits+1;
			
			MPUnit sum = new MPUnit();
			sum.Digits = maxdigits+1;

			int i=0;

			// The result may be 1 greater than
			// maxdigits because of carry, this is overflow
			for(i=0; i<=maxdigits; i++)
			{
				System.UInt32 us = (UInt32)(summand1[i]+summand2[i]+carry);
				carry = (UInt16)(us >> 16);
				sum[i] = (UInt16)(us & 0xffff);
			}

			while(i>0 && sum[i-1]==0)
			{	i--;  }

			sum.numDigits = i;

			// overflow
			if(carry != 0)
				throw new ArithmeticException();

			return sum;
		}


		public static MPUnit Sub(MPUnit minuend, MPUnit subtrahend)
		{
			// subtrahend >= minuend
			int compares = minuend.CompareTo(subtrahend);
			if(compares == 0)
			{
				MPUnit retval = new MPUnit();
				retval.numDigits = minuend.numDigits;
				return retval;
			}

			if(compares<0)
				throw new ArithmeticException();

			MPUnit difference = new MPUnit();
			int maxdigits = minuend.Digits;
			subtrahend.Digits=maxdigits;

			int carry=0;
		
			for(int i=0; i<maxdigits; i++)
			{
				int resdigit = minuend[i]-subtrahend[i]+carry;
				if(resdigit<0)
				{
					resdigit+=MPUnit.BASE;
					carry=-1;
				}
				else
					carry=0;
				difference[i]=(UInt16)resdigit;
			}


			while(difference.Digits >0 && difference[difference.Digits-1]==0)
			{	difference.Digits = difference.Digits-1;  }

			// underflow exception
			if(carry==-1)
				throw new ArithmeticException();

			return difference;
		}

		/// <summary>
		/// Multiplies current MPUnit with um1
		/// </summary>
		/// <param name="um1"></param>
		/// <returns>MPUnit</returns>
		public static MPUnit Mult(MPUnit s1, System.UInt16 um1)
		{
			MPUnit product = new MPUnit(s1.maxDigits);
			System.UInt16 carry = 0;
			System.UInt32 um2 = 0;
			int i=0;

			for(i=0; i < s1.Digits; i++)
			{
				um2 = (UInt32)(s1[i]*um1)+carry;
				carry=(UInt16)(um2>>16);
				product[i]=(System.UInt16)(um2 & 0xffff);
			}

			if(carry!=0)
			{
				if(i==s1.maxDigits)
					throw new ArithmeticException();
				product[i++]=carry;
			}
#if UNDONE
			while(i >0 && product[i-1]==0)
				i--;
#endif
			product.numDigits=i;

			return product;

		}

		/// <summary>
		/// Multiplies current MPUnit with mpu1
		/// </summary>
		/// <param name="mpu1"></param>
		/// <returns>MPUnit</returns>
		public static MPUnit Mult(MPUnit factor1, MPUnit factor2)
		{
			UInt16[] productdigits = new UInt16[MPUnit.NormalSize*2];
			MPUnit product = new MPUnit();
			int i=0;
			int carry=0;

			foreach(UInt16 ui in factor1.digitBlocks)
			{
				int j=0;
				foreach(UInt16 vj in factor2.digitBlocks)
				{
					System.UInt32 up = (UInt32)(ui*vj+carry+productdigits[i+j]);
					productdigits[i+j] = (UInt16)(up & 0xffff);
					carry=(UInt16)(up>>16);
					j++;
				}
				i++;
			}

			int ndig = factor1.Digits+factor2.Digits;
#if UNDONE
			while(ndig>0 && productdigits[ndig-1]==0)
				ndig--;
#endif
			product.numDigits=ndig;
			Array.Copy(productdigits,0,product.digitBlocks,0,Math.Min(ndig,MPUnit.NormalSize));

			// overflow
			if(ndig > MPUnit.NormalSize)
				throw new ArithmeticException();

			return product;
		}

		/// <summary>
		/// Divides current MPUnit by mpu1, Quotient in q, Remainder in r
		/// </summary>
		/// <param name="mpu1"></param>
		/// <param name="q"></param>
		/// <param name="r"></param>
		/// <returns></returns>
		public static void Div(MPUnit dividend, MPUnit divisor, ref MPUnit q, ref MPUnit r)
		{
			MPUnit mpuInter = new MPUnit();
			MPUnit u = new MPUnit(dividend);
			MPUnit v = new MPUnit(divisor);
			q = new MPUnit();

			// Can't divide by zero
			if(v.numDigits == 0)
				throw new ArgumentException();

			// dividing zero by something else equals zero
			if(u.numDigits == 0)
			{
				q.numDigits=0;
				r.numDigits=0;
				return;
			}

			// if the v only has one digit, use the simple routine
			// BUGBUG, how about pretending v has two digits, then postshifting?
			if(v.numDigits == 1)
			{
				Div(u, v[0], ref q , ref r);
				return;
			}

			// ok, long route
			// normalize to give better qhat estimates
			// this raises number of digits in u by one
			// (top digit may be zero) and does not raise the number
			// of digits in v (since we've just scaled its top
			// digit to be between BASE/2 and BASE
			UInt16 scale = 1;
			
			int n = v.Digits;
			int m = u.Digits-n;

			int v_msd = v[n-1];

			// scale up v
			while(v_msd < BASE/2)
			{
				v_msd <<= 1;
				scale <<= 1;
			}

			// if no shift occurs, or if the multiplication
			// doesn't cause a carry into a higher digit
			// we will add an additional 0 digit anyway
			int u_inc_digits = u.Digits+1;

			if(scale != 1)
			{
				// This may or may not increment the number of digits in u... 
				// must check this
				int digits = u.Digits;
				u = MPUnit.Mult(u,scale);
				v = MPUnit.Mult(v,scale);
			}

			u.Digits=u_inc_digits;

			// initialize j
			for(int j=m; j>=0; j--)
			{
				// generate qhat
				// From Knuth (Uj+nB + Uj+n-1)/(Vn-1)
				long uhat = (((long)u[j+n]) << 16) + ((long)u[j+n-1]) ;
				long vhat = (long)v[n-1];
				long qhat = uhat  / vhat ;
				long rhat = uhat - (qhat * vhat);

				long test1 = qhat*v[n-2];
				long test2 = (BASE * rhat) + ( (j+n-2) >=0 ? (int)u[j+n-2] : (int)0);

				// Make sure we didn't overflow in
				// creating the test values
				Debug.Assert(test1>=0 && test2>=0);

				// decrease qhat by one if it is BASE or test fails
				if(qhat == BASE || test1 > test2)
				{
					qhat--;
					rhat += v[n-1];
					test1 = qhat*v[n-2];
					test2 = (BASE * rhat) + ( (j+n-2) >=0 ? (int)u[j+n-2] : (int)0);

					// qhat is still 1 too great
					if(rhat < BASE && (qhat == BASE || test1>test2))
					{
						qhat--;
					}

				}

				Debug.Assert(qhat < BASE && qhat>=0 && rhat>=0);

				// Multiply and subtract
				// subtract term from top term.Digits digits of u
				// easiest done as a shift of term?

				MPUnit term = MPUnit.Mult(v,(ushort)qhat);
				term.ASL(j);

				// if the result would be negative, then
				// we oopsd again
				if(u.CompareTo(term)<0)
				{
					qhat--;
					term = MPUnit.Mult(v,(ushort)qhat);
					term.ASL(j);
				}


				u = MPUnit.Sub(u,term);

				// set quotient digit
				q[j]=(ushort)qhat;

			}

			q.Trim();
			r = MPUnit.Sub(dividend,MPUnit.Mult(divisor,q));
			r.Trim();

			return;
		}

		/// <summary>
		/// Divides current MPUnit by short int ui, Quotient in q, Remainder in r
		/// </summary>
		/// <param name="i"></param>
		/// <param name="q"></param>
		/// <param name="r"></param>
		/// <returns></returns>
		public static void Div(MPUnit dividend, int i, ref MPUnit q, ref MPUnit r)
		{
			if(i < 0 || i > BASE)
				throw new ArgumentOutOfRangeException();

			MPUnit pseudodividend = new MPUnit(dividend);
			pseudodividend.ASL(1);
			MPUnit divisor = new MPUnit(i);
			MPUnit pseudodivisor = new MPUnit(divisor);
			pseudodivisor.ASL(1);
			Div(pseudodividend, pseudodivisor, ref q, ref r);
			
			r = MPUnit.Sub(dividend, MPUnit.Mult(q,divisor));
		}

		/// <summary>
		/// CompareTo compares current MPUnit to objCompareTo
		/// </summary>
		/// <param name="objCompareTo"></param>
		/// <returns></returns>
		public int CompareTo(object objCompareTo)
		{
			MPUnit mpu1 = objCompareTo as MPUnit;
			int mydigits = numDigits;
			int otherdigits = mpu1.Digits;

			// strip leading zeros
			while(mydigits >0 && this[mydigits-1]==0)
				mydigits--;
			while(otherdigits >0 && mpu1[otherdigits-1]==0)
				otherdigits--;

			if(mydigits == otherdigits && otherdigits==0)
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
				int i=mydigits-1;
				while(i>=0)
				{
					if(this[i]!=mpu1[i])
						return (this[i]>mpu1[i] ? 1 : -1);
					i--;
				}
			}

			return 0;
		}

		private string OutputBaseFormatter()
		{
			string outputstring = null;

			switch(outputFormatBase)
			{
				case OutputFormatBase.Dec:
					outputstring = FormatDec();
					break;
				case OutputFormatBase.Hex:
					break;
				case OutputFormatBase.Binary:
					break;
				case OutputFormatBase.Octal:
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			return outputstring;
		}

		private string FormatDec()
		{
			MPUnit tempUnit = new MPUnit(this);
			MPUnit q=null;
			MPUnit r=null;
			ArrayList cc = new ArrayList();
			while(tempUnit.Digits>0)
			{
				MPUnit.Div(tempUnit, 10, ref q, ref r);
				if(r.Digits>0)
					cc.Add(r[0].ToString());
				tempUnit = q;
			}

			cc.Reverse();

			StringBuilder sb = new StringBuilder();
			foreach(string digstring in cc)
			{
				sb.Append(digstring);
			}

			return sb.ToString();
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			if(numDigits==0)
			{
				sb.AppendFormat("0");
			}
			else
			for(int i=numDigits-1; i>=0; i--)
			{
				sb.AppendFormat("{0},",this[i]);
			}
			return sb.ToString();


		}
	}
}
