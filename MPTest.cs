using System;
using System.Collections;
using System.Text;
using System.Diagnostics;
using MPMath;

public class MPIntTest 
{

	static void Main(string[] args) 
	{
		Console.WriteLine(DateTime.Now);
		Console.WriteLine("Simple clone test");
    
		// Clone test
		MPInt r1 = (long)(25*65536+15);
		r1.DisplayBase = DisplayBases.Dec;
    
		MPInt r2 = (MPInt)r1.Clone();
		Console.WriteLine("These should be the same {0}, {1}",r1.ToString(), r2.ToString());
		r2+= 12<<1;
		Console.WriteLine("R1 should be unchanged: {0}, but R2=R1+24: {1}",r1.ToString(), r2.ToString());
    
		Console.WriteLine("Simple addition test");
    
		MPInt mpu1 = new MPInt();
		mpu1 += 65535;
		Console.WriteLine("mpu1 should be 65535: is {0}",mpu1);
    
		MPInt mpu2 = new MPInt();
		mpu2++;
    
		Console.WriteLine("mpu2 should be 1: is {0}",mpu2);
		int i;
    
		for( i = 0; i<20; i++)
		{
			mpu1 += mpu2;
		}
    
		Console.WriteLine("mpu1 should be : {0}, is {1}",65535+20,mpu1);
    
		Console.WriteLine("Simple subtraction test");
		Console.WriteLine("i: {0}, mpu1: {1}",i, mpu1);
    
		for(i=1; i<21; i++)
		{
			mpu1 -= mpu2;
			Console.WriteLine("i: {0}, mpu1: {1}",i, mpu1);
		}

		Console.WriteLine("Simple addition test");
    
		for(i=1; i<21; i++)
		{
			mpu1 += mpu2;
			Console.WriteLine("i: {0}, mpu1: {1}",i, mpu1);
		}
    
    
		MPInt mpu3 = (long)(65536);
		Console.WriteLine("Simple multiplication test, start with {0}",mpu3.ToString());
    
		mpu3.DisplayBase = DisplayBases.Hex;
		MPInt mpu4 = (long)1;
    
		for(i=0; i<20; i++)
		{
			Console.WriteLine("mpu3 {0} :: mpu4 {1}",mpu3, mpu4);
			mpu4++;				
			mpu3 *= mpu4;
		}
    
		Console.WriteLine("Simple division test");
    
		for(i=0; i<21; i++)
		{
			mpu3.DisplayBase = DisplayBases.Hex;
			Console.WriteLine("mpu3 {0} :: mpu4 {1}",mpu3, mpu4);
			mpu3 /= mpu4;
			mpu4--;				
		}
    
		Console.WriteLine("Simple square test");
		Console.WriteLine("'Fast'? square test");
    
		MPInt mpuToSqr=0;
		long start1 = DateTime.Now.Ticks;
		for(int j=0; j<50; j++)
		{
			mpuToSqr = (long)625;
			for(i=0; i<5; i++)
			{
				mpuToSqr = MPInt.Sqr(mpuToSqr);
			}
		}
		TimeSpan ts = new TimeSpan(DateTime.Now.Ticks - start1);
		Console.WriteLine("Square Time: {0}:{1}",ts.Seconds,ts.Milliseconds);
		Console.WriteLine(mpuToSqr);
#if SLOW
    Console.WriteLine("'Slow'? square test");
    
    
    start1 = DateTime.Now.Ticks;
    for(int j=0; j<50; j++){
      mpuToSqr = (long)625;
      for(i=0; i<5; i++){
	mpuToSqr = MPInt.SlowSqr(mpuToSqr);
      }
    }
    Console.WriteLine((DateTime.Now.Ticks - start1).ToString());
    Console.WriteLine(mpuToSqr);
#endif
		Console.WriteLine("Mod2 test");
		MPInt mpuToMod = (long)65535;
		mpuToMod = MPInt.Sqr(mpuToMod);
		Console.WriteLine("65535 squared is: {0}",mpuToMod);
		MPInt modded = MPInt.Mod2(mpuToMod,20);
		Console.WriteLine("{0} mod 2^20 is {1}",mpuToMod,modded);
    
		Console.WriteLine("Mod test");
		mpuToMod = (long)65535;
		mpuToMod = MPInt.Sqr(mpuToMod);
		Console.WriteLine("65535 squared is: {0}",mpuToMod);
		mpuToMod = MPInt.Sqr(mpuToMod);
		Console.WriteLine("and that squared is: {0}",mpuToMod);
		Console.WriteLine("and mod 65535, which should be 0 is {0}",MPInt.Mod(mpuToMod,65535));
    
		Console.WriteLine("FactPair test");
		TwoFact tf = new TwoFact();
		for(int ipow=0; ipow< 64; ipow++)
		{
			Console.WriteLine("Two Power: {0}, Odd Power: {1}",tf[ipow].twofact,tf[ipow].oddfact);
		}
    
		Console.WriteLine("Number of binary and base K digit test");
		MPInt digTest = (long)6670019976;
		Console.WriteLine(digTest);
		Console.WriteLine("Number of base 16 digits: {0}", digTest.Length);
		Console.WriteLine("Number of binary digits: {0}", digTest.NumBinaryDigits);
		digTest.DisplayBase = DisplayBases.Hex;
		Console.WriteLine(digTest);
		Console.WriteLine("Number of base K digits: {0}", digTest.Num5Digits);
    
		Console.WriteLine(DateTime.Now);    
	}
  
}
