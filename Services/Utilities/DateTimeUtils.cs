﻿namespace CoursesAPI.Services.Utilities
{
	public class DateTimeUtils
	{
		public static bool IsLeapYear(int year)
		{
			if(year%4 == 0 && year%100 != 0)
            {
                return true;
            }
            if(year%4 == 0 && year%100 == 0 && year%400 == 0)
            {
                return true;
            }
            // TODO: add your logic here!!!1!!!one!!!eleven
			return false;
		}
	}
}
