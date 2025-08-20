namespace MAME.Core
{
    public struct Atime
    {
        public int seconds;
        public long attoseconds;
        public Atime(int i, long l)
        {
            seconds = i;
            attoseconds = l;
        }
    }
    public class Attotime
    {
        public static int ATTOTIME_MAX_SECONDS = 1000000000, ATTOSECONDS_PER_SECOND_SQRT = 1000000000;
        public static long ATTOSECONDS_PER_SECOND = (long)(1e18);
        public static Atime ATTOTIME_ZERO = new Atime(0, 0);
        public static Atime ATTOTIME_NEVER = new Atime(1000000000, 0);
        public static long ATTOSECONDS_PER_NANOSECOND = (long)1e9;
        public static Atime ATTOTIME_IN_NSEC(long ns)
        {
            return new Atime((int)(ns / 1000000000), (long)((ns % 1000000000) * ATTOSECONDS_PER_NANOSECOND));
        }
        public static Atime ATTOTIME_IN_HZ(int hz)
        {
            return new Atime(0, (long)(ATTOSECONDS_PER_SECOND / hz));
        }
        public static long attotime_to_attoseconds(Atime _time)
        {
            if (_time.seconds == 0)
            {
                return _time.attoseconds;
            }
            else if (_time.seconds == -1)
            {
                return _time.attoseconds - Attotime.ATTOSECONDS_PER_SECOND;
            }
            else if (_time.seconds > 0)
            {
                return Attotime.ATTOSECONDS_PER_SECOND;
            }
            else
            {
                return -Attotime.ATTOSECONDS_PER_SECOND;
            }
        }
        public static Atime attotime_add(Atime _time1, Atime _time2)
        {
            Atime result = new Atime();

            /* if one of the items is attotime_never, return attotime_never */
            if (_time1.seconds >= ATTOTIME_MAX_SECONDS || _time2.seconds >= ATTOTIME_MAX_SECONDS)
                return ATTOTIME_NEVER;

            /* add the seconds and attoseconds */
            result.attoseconds = _time1.attoseconds + _time2.attoseconds;
            result.seconds = _time1.seconds + _time2.seconds;

            /* normalize and return */
            if (result.attoseconds >= ATTOSECONDS_PER_SECOND)
            {
                result.attoseconds -= ATTOSECONDS_PER_SECOND;
                result.seconds++;
            }

            /* overflow */
            if (result.seconds >= ATTOTIME_MAX_SECONDS)
                return ATTOTIME_NEVER;
            return result;
        }
        public static Atime attotime_add_attoseconds(Atime _time1, long _attoseconds)
        {
            Atime result;

            /* if one of the items is attotime_never, return attotime_never */
            if (_time1.seconds >= ATTOTIME_MAX_SECONDS)
                return ATTOTIME_NEVER;

            /* add the seconds and attoseconds */
            result.attoseconds = _time1.attoseconds + _attoseconds;
            result.seconds = _time1.seconds;

            /* normalize and return */
            if (result.attoseconds >= ATTOSECONDS_PER_SECOND)
            {
                result.attoseconds -= ATTOSECONDS_PER_SECOND;
                result.seconds++;
            }

            /* overflow */
            if (result.seconds >= ATTOTIME_MAX_SECONDS)
                return ATTOTIME_NEVER;
            return result;
        }
        public static Atime attotime_sub(Atime _time1, Atime _time2)
        {
            Atime result;

            /* if time1 is attotime_never, return attotime_never */
            if (_time1.seconds >= ATTOTIME_MAX_SECONDS)
                return ATTOTIME_NEVER;

            /* add the seconds and attoseconds */
            result.attoseconds = _time1.attoseconds - _time2.attoseconds;
            result.seconds = _time1.seconds - _time2.seconds;

            /* normalize and return */
            if (result.attoseconds < 0)
            {
                result.attoseconds += ATTOSECONDS_PER_SECOND;
                result.seconds--;
            }
            return result;
        }
        public static Atime attotime_sub_attoseconds(Atime _time1, long _attoseconds)
        {
            Atime result;

            /* if time1 is attotime_never, return attotime_never */
            if (_time1.seconds >= ATTOTIME_MAX_SECONDS)
                return ATTOTIME_NEVER;

            /* add the seconds and attoseconds */
            result.attoseconds = _time1.attoseconds - _attoseconds;
            result.seconds = _time1.seconds;

            /* normalize and return */
            if (result.attoseconds < 0)
            {
                result.attoseconds += ATTOSECONDS_PER_SECOND;
                result.seconds--;
            }
            return result;
        }
        public static Atime attotime_mul(Atime _time1, uint factor)
        {
            uint attolo, attohi, reslo, reshi;
            ulong temp;

            /* if one of the items is attotime_never, return attotime_never */
            if (_time1.seconds >= ATTOTIME_MAX_SECONDS)
                return ATTOTIME_NEVER;

            /* 0 times anything is zero */
            if (factor == 0)
                return ATTOTIME_ZERO;

            /* split attoseconds into upper and lower halves which fit into 32 bits */
            attohi = divu_64x32_rem((ulong)_time1.attoseconds, 1000000000, out attolo);

            /* scale the lower half, then split into high/low parts */
            temp = mulu_32x32(attolo, factor);
            temp = divu_64x32_rem(temp, 1000000000, out reslo);

            /* scale the upper half, then split into high/low parts */
            temp += mulu_32x32(attohi, factor);
            temp = divu_64x32_rem(temp, 1000000000, out reshi);

            /* scale the seconds */
            temp += mulu_32x32((uint)_time1.seconds, factor);
            if (temp >= 1000000000)
                return ATTOTIME_NEVER;

            /* build the result */
            return new Atime((int)temp, (long)reslo + mul_32x32((int)reshi, 1000000000));
        }
        private static uint divu_64x32_rem(ulong a, uint b, out uint remainder)
        {
            remainder = (uint)(a % (ulong)b);
            return (uint)(a / (ulong)b);
        }
        private static ulong mulu_32x32(uint a, uint b)
        {
            return (ulong)a * (ulong)b;
        }
        private static long mul_32x32(int a, int b)
        {
            return (long)a * (long)b;
        }
        public static Atime attotime_div(Atime _time1, uint factor)
        {
            uint attolo, attohi, reshi, reslo, remainder;
            Atime result;
            ulong temp;

            /* if one of the items is attotime_never, return attotime_never */
            if (_time1.seconds >= ATTOTIME_MAX_SECONDS)
                return new Atime(ATTOTIME_MAX_SECONDS, 0);

            /* ignore divide by zero */
            if (factor == 0)
                return _time1;

            /* split attoseconds into upper and lower halves which fit into 32 bits */
            attohi = divu_64x32_rem((ulong)_time1.attoseconds, 1000000000, out attolo);

            /* divide the seconds and get the remainder */
            result.seconds = (int)divu_64x32_rem((ulong)_time1.seconds, factor, out remainder);

            /* combine the upper half of attoseconds with the remainder and divide that */
            temp = (ulong)attohi + mulu_32x32(remainder, 1000000000);
            reshi = divu_64x32_rem(temp, factor, out remainder);

            /* combine the lower half of attoseconds with the remainder and divide that */
            temp = attolo + mulu_32x32(remainder, 1000000000);
            reslo = divu_64x32_rem(temp, factor, out remainder);

            /* round based on the remainder */
            result.attoseconds = (long)reslo + (long)mulu_32x32(reshi, 1000000000);
            if (remainder >= factor / 2)
                if (++result.attoseconds >= ATTOSECONDS_PER_SECOND)
                {
                    result.attoseconds = 0;
                    result.seconds++;
                }
            return result;
        }
        public static int attotime_compare(Atime _time1, Atime _time2)
        {
            if (_time1.seconds > _time2.seconds)
                return 1;
            if (_time1.seconds < _time2.seconds)
                return -1;
            if (_time1.attoseconds > _time2.attoseconds)
                return 1;
            if (_time1.attoseconds < _time2.attoseconds)
                return -1;
            return 0;
        }
    }
}