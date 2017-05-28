﻿using Pk3DSRNGTool.Core;

namespace Pk3DSRNGTool
{
    public class Stationary7 : StationaryRNG
    {
        private static ulong getrand => RNGPool.getrand64;
        private static void time_elapse(int n) => RNGPool.time_elapse(n);
        private static void Advance(int n) => RNGPool.Advance(n);

        public bool blinkwhensync;

        private bool blink_process()
        {
            bool sync = (int)(getrand % 100) >= 50;
            if (blinkwhensync)
                time_elapse(3);
            return sync;
        }

        public override RNGResult Generate()
        {
            Result7 rt = new Result7();
            rt.Level = Level;

            //Synchronize
            if (AlwaysSync)
                rt.Synchronize = true;
            else
            {
                rt.Synchronize = blink_process();
                Advance(60);
            }

            //Encryption Constant
            rt.EC = (uint)(getrand & 0xFFFFFFFF);

            //PID
            for (int i = PIDroll_count; i > 0; i--)
            {
                rt.PID = (uint)(getrand & 0xFFFFFFFF);
                if (rt.PSV == TSV)
                {
                    if (IsShinyLocked)
                        rt.PID ^= 0x10000000;
                    else
                        rt.Shiny = true;
                    break;
                }
            }

            //IV
            rt.IVs = (int[])IVs.Clone();
            for (int i = PerfectIVCount; i > 0;)
            {
                int tmp = (int)(getrand % 6);
                if (rt.IVs[tmp] < 0)
                {
                    i--; rt.IVs[tmp] = 31;
                }
            }
            for (int i = 0; i < 6; i++)
                if (rt.IVs[i] < 0)
                    rt.IVs[i] = (int)(getrand & 0x1F);

            //Ability
            rt.Ability = (byte)(blinkwhensync ? 1 : (getrand & 1) + 1);

            //Nature
            rt.Nature = (byte)(rt.Synchronize && Synchro_Stat < 25 ? Synchro_Stat : getrand % 25);

            //Gender
            rt.Gender = (byte)(RandomGender ? ((int)(getrand % 252) >= Gender ? 1 : 2) : Gender);

            return rt;
        }

        public RNGResult GenerateMainRNGPID(EggResult egg)
        {
            MainRNGEgg rt = new MainRNGEgg(egg);
            rt.PID = (uint)(getrand & 0xFFFFFFFF);
            rt.Shiny = rt.PSV == TSV;
            return rt;
        }

        public override void UseTemplate(Pokemon PM)
        {
            base.UseTemplate(PM);
            blinkwhensync = !AlwaysSync && !(PM as PKM7).NoBlink;
        }
    }
}
