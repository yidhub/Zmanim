﻿// * Zmanim .NET API
// * Copyright (C) 2004-2010 Eliyahu Hershfeld
// *
// * Converted to C# by AdminJew
// *
// * This file is part of Zmanim .NET API.
// *
// * Zmanim .NET API is free software: you can redistribute it and/or modify
// * it under the terms of the GNU Lesser General Public License as published by
// * the Free Software Foundation, either version 3 of the License, or
// * (at your option) any later version.
// *
// * Zmanim .NET API is distributed in the hope that it will be useful,
// * but WITHOUT ANY WARRANTY; without even the implied warranty of
// * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// * GNU Lesser General Public License for more details.
// *
// * You should have received a copy of the GNU Lesser General Public License
// * along with Zmanim.NET API.  If not, see <http://www.gnu.org/licenses/lgpl.html>.

using System;
using java.util;
using Zmanim.Extensions;

namespace net.sourceforge.zmanim.util
{
    /// <summary>
    ///   Implementation of sunrise and sunset methods to calculate astronomical times based on the <a href = "http://noaa.gov">NOAA</a> algorithm.
    ///   This calculator uses the Java algorithm based on the implementation by <a href = "http://noaa.gov">NOAA - National Oceanic and Atmospheric
    ///                                                                            Administration</a>'s <a href = "http://www.srrb.noaa.gov/highlights/sunrise/sunrisehtml">Surface Radiation
    ///                                                                                                   Research Branch</a>. NOAA's <a href = "http://www.srrb.noaa.gov/highlights/sunrise/solareqns.PDF">implementation</a>
    ///   is based on equations from <a href = "http://www.willbell.com/math/mc1.htm">Astronomical Algorithms</a> by
    ///   <a href = "http://en.wikipedia.org/wiki/Jean_Meeus">Jean Meeus</a>. Added to
    ///   the algorithm is an adjustment of the zenith to account for elevation.
    /// </summary>
    /// <author>Eliyahu Hershfeld</author>
    public class NOAACalculator : AstronomicalCalculator
    {
        private string calculatorName = "US National Oceanic and Atmospheric Administration Algorithm";

        public override string getCalculatorName()
        {
            return calculatorName;
        }

        ///<seealso cref = "net.sourceforge.zmanim.util.AstronomicalCalculator.getUTCSunrise(AstronomicalCalendar,double, bool)" />
        public override double getUTCSunrise(AstronomicalCalendar astronomicalCalendar, double zenith,
                                             bool adjustForElevation)
        {
            //		if (astronomicalCalendar.getCalendar().get(Calendar.YEAR) <= 2000) {
            //			throw new ZmanimException(
            //					"NOAACalculator can not calculate times earlier than the year 2000.	Please try a date with a different year.");
            //		}

            if (adjustForElevation)
            {
                zenith = adjustZenith(zenith, astronomicalCalendar.getGeoLocation().getElevation());
            }
            else
            {
                zenith = adjustZenith(zenith, 0);
            }

            double sunRise = calcSunriseUTC(calcJD(astronomicalCalendar.getCalendar()),
                                            astronomicalCalendar.getGeoLocation().getLatitude(),
                                            -astronomicalCalendar.getGeoLocation().getLongitude(), zenith);
            return sunRise/60;
        }

        ///<seealso cref = "net.sourceforge.zmanim.util.AstronomicalCalculator.getUTCSunset(AstronomicalCalendar,double, bool)" />
        ///<exception cref = "ZmanimException">
        ///  If the year entered is lt 2001.
        ///  This calculator can't properly deal with the year 2000.
        ///  It can properly calculate times for years gt 2000. </exception>
        public override double getUTCSunset(AstronomicalCalendar astronomicalCalendar, double zenith,
                                            bool adjustForElevation)
        {
            // if (astronomicalCalendar.getCalendar().get(Calendar.YEAR) <= 2000) {
            // throw new ZmanimException(
            // "NOAACalculator can not calculate times for the year 2000. Please try
            // a date with a different year.");
            // }

            if (adjustForElevation)
            {
                zenith = adjustZenith(zenith, astronomicalCalendar.getGeoLocation().getElevation());
            }
            else
            {
                zenith = adjustZenith(zenith, 0);
            }

            double sunSet = calcSunsetUTC(calcJD(astronomicalCalendar.getCalendar()),
                                          astronomicalCalendar.getGeoLocation().getLatitude(),
                                          -astronomicalCalendar.getGeoLocation().getLongitude(), zenith);
            return sunSet/60;
        }

        ///<summary>
        ///  Generate a Julian day from Java Calendar
        ///</summary>
        ///<param name = "date">
        ///  Java Calendar </param>
        ///<returns> the Julian day corresponding to the date Note: Number is returned
        ///  for start of day. Fractional days should be added later. </returns>
        private static double calcJD(ICalendar date)
        {
            int year = date.getTime().Year;
            int month = date.getTime().Month + 1;
            int day = date.getTime().Day;
            if (month <= 2)
            {
                year -= 1;
                month += 12;
            }
            double A = Math.Floor((double) (year/100));
            double B = 2 - A + Math.Floor(A/4);

            return Math.Floor(365.25*(year + 4716)) + Math.Floor(30.6001*(month + 1)) + day + B - 1524.5;
        }

        ///<summary>
        ///  convert Julian Day to centuries since J2000.0.
        ///</summary>
        ///<param name = "jd">
        ///  the Julian Day to convert </param>
        ///<returns> the T value corresponding to the Julian Day </returns>
        private static double calcTimeJulianCent(double jd)
        {
            return (jd - 2451545.0)/36525.0;
        }

        ///<summary>
        ///  Convert centuries since J2000.0 to Julian Day.
        ///</summary>
        ///<param name = "t">
        ///  the number of Julian centuries since J2000.0 </param>
        ///<returns> the Julian Day corresponding to the t value </returns>
        private static double calcJDFromJulianCent(double t)
        {
            return t*36525.0 + 2451545.0;
        }

        ///<summary>
        ///  calculates the Geometric Mean Longitude of the Sun
        ///</summary>
        ///<param name = "t">
        ///  the number of Julian centuries since J2000.0 </param>
        ///<returns> the Geometric Mean Longitude of the Sun in degrees </returns>
        private static double calcGeomMeanLongSun(double t)
        {
            double L0 = 280.46646 + t*(36000.76983 + 0.0003032*t);
            while (L0 > 360.0)
            {
                L0 -= 360.0;
            }
            while (L0 < 0.0)
            {
                L0 += 360.0;
            }

            return L0; // in degrees
        }

        ///<summary>
        ///  Calculate the Geometric Mean Anomaly of the Sun
        ///</summary>
        ///<param name = "t">
        ///  the number of Julian centuries since J2000.0 </param>
        ///<returns> the Geometric Mean Anomaly of the Sun in degrees </returns>
        private static double calcGeomMeanAnomalySun(double t)
        {
            double M = 357.52911 + t*(35999.05029 - 0.0001537*t);
            return M; // in degrees
        }

        ///<summary>
        ///  calculate the eccentricity of earth's orbit
        ///</summary>
        ///<param name = "t">
        ///  the number of Julian centuries since J2000.0 </param>
        ///<returns> the unitless eccentricity </returns>
        private static double calcEccentricityEarthOrbit(double t)
        {
            double e = 0.016708634 - t*(0.000042037 + 0.0000001267*t);
            return e; // unitless
        }

        ///<summary>
        ///  Calculate the equation of center for the sun
        ///</summary>
        ///<param name = "t">
        ///  the number of Julian centuries since J2000.0 </param>
        ///<returns> the equation of center for the sun in degrees </returns>
        private static double calcSunEqOfCenter(double t)
        {
            double m = calcGeomMeanAnomalySun(t);

            double mrad = MathExtensions.ToRadians(m);
            double sinm = Math.Sin(mrad);
            double sin2m = Math.Sin(mrad + mrad);
            double sin3m = Math.Sin(mrad + mrad + mrad);

            double C = sinm*(1.914602 - t*(0.004817 + 0.000014*t)) + sin2m*(0.019993 - 0.000101*t) + sin3m*0.000289;
            return C; // in degrees
        }

        ///<summary>
        ///  Calculate the true longitude of the sun
        ///</summary>
        ///<param name = "t">
        ///  the number of Julian centuries since J2000.0 </param>
        ///<returns> the sun's true longitude in degrees </returns>
        private static double calcSunTrueLong(double t)
        {
            double l0 = calcGeomMeanLongSun(t);
            double c = calcSunEqOfCenter(t);

            double O = l0 + c;
            return O; // in degrees
        }

        //	/**
        //	 * Calculate the true anamoly of the sun
        //	 *
        //	 * @param t
        //	 *            the number of Julian centuries since J2000.0
        //	 * @return the sun's true anamoly in degrees
        //	 */
        //	private static double calcSunTrueAnomaly(double t) {
        //		double m = calcGeomMeanAnomalySun(t);
        //		double c = calcSunEqOfCenter(t);
        //
        //		double v = m + c;
        //		return v; // in degrees
        //	}

        ///<summary>
        ///  calculate the apparent longitude of the sun
        ///</summary>
        ///<param name = "t">
        ///  the number of Julian centuries since J2000.0 </param>
        ///<returns> sun's apparent longitude in degrees </returns>
        private static double calcSunApparentLong(double t)
        {
            double o = calcSunTrueLong(t);

            double omega = 125.04 - 1934.136*t;
            double lambda = o - 0.00569 - 0.00478*Math.Sin(MathExtensions.ToRadians(omega));
            return lambda; // in degrees
        }

        ///<summary>
        ///  Calculate the mean obliquity of the ecliptic
        ///</summary>
        ///<param name = "t">
        ///  the number of Julian centuries since J2000.0 </param>
        ///<returns> the mean obliquity in degrees </returns>
        private static double calcMeanObliquityOfEcliptic(double t)
        {
            double seconds = 21.448 - t*(46.8150 + t*(0.00059 - t*(0.001813)));
            double e0 = 23.0 + (26.0 + (seconds/60.0))/60.0;
            return e0; // in degrees
        }

        ///<summary>
        ///  calculate the corrected obliquity of the ecliptic
        ///</summary>
        ///<param name = "t">
        ///  the number of Julian centuries since J2000.0 </param>
        ///<returns> the corrected obliquity in degrees </returns>
        private static double calcObliquityCorrection(double t)
        {
            double e0 = calcMeanObliquityOfEcliptic(t);

            double omega = 125.04 - 1934.136*t;
            double e = e0 + 0.00256*Math.Cos(MathExtensions.ToRadians(omega));
            return e; // in degrees
        }

        ///<summary>
        ///  Calculate the declination of the sun
        ///</summary>
        ///<param name = "t">
        ///  the number of Julian centuries since J2000.0 </param>
        private static double calcSunDeclination(double t)
        {
            double e = calcObliquityCorrection(t);
            double lambda = calcSunApparentLong(t);

            double sint = Math.Sin(MathExtensions.ToRadians(e))*Math.Sin(MathExtensions.ToRadians(lambda));
            double theta = MathExtensions.ToDegree(Math.Asin(sint));
            return theta; // in degrees
        }

        ///<summary>
        ///  calculate the difference between true solar time and mean solar time
        ///</summary>
        ///<param name = "t">
        ///  the number of Julian centuries since J2000.0 </param>
        ///<returns> equation of time in minutes of time </returns>
        private static double calcEquationOfTime(double t)
        {
            double epsilon = calcObliquityCorrection(t);
            double l0 = calcGeomMeanLongSun(t);
            double e = calcEccentricityEarthOrbit(t);
            double m = calcGeomMeanAnomalySun(t);

            double y = Math.Tan(MathExtensions.ToRadians(epsilon)/2.0);
            y *= y;

            double sin2l0 = Math.Sin(2.0*MathExtensions.ToRadians(l0));
            double sinm = Math.Sin(MathExtensions.ToRadians(m));
            double cos2l0 = Math.Cos(2.0*MathExtensions.ToRadians(l0));
            double sin4l0 = Math.Sin(4.0*MathExtensions.ToRadians(l0));
            double sin2m = Math.Sin(2.0*MathExtensions.ToRadians(m));

            double Etime = y*sin2l0 - 2.0*e*sinm + 4.0*e*y*sinm*cos2l0 - 0.5*y*y*sin4l0 - 1.25*e*e*sin2m;
            return MathExtensions.ToDegree(Etime)*4.0; // in minutes of time
        }

        ///<summary>
        ///  Calculate the hour angle of the sun at sunrise for the latitude
        ///</summary>
        ///<param name = "lat">,
        ///  the latitude of observer in degrees </param>
        ///<param name = "solarDec">
        ///  the declination angle of sun in degrees </param>
        ///<returns> hour angle of sunrise in radians </returns>
        private static double calcHourAngleSunrise(double lat, double solarDec, double zenith)
        {
            double latRad = MathExtensions.ToRadians(lat);
            double sdRad = MathExtensions.ToRadians(solarDec);

            // double HAarg =
            // (Math.cos(MathExtensions.ToRadians(zenith))/(Math.cos(latRad)*Math.cos(sdRad))-Math.tan(latRad)
            // * Math.tan(sdRad));

            double HA =
                (Math.Acos(Math.Cos(MathExtensions.ToRadians(zenith))/(Math.Cos(latRad)*Math.Cos(sdRad)) -
                           Math.Tan(latRad)*Math.Tan(sdRad)));
            return HA; // in radians
        }

        ///<summary>
        ///  Calculate the hour angle of the sun at sunset for the latitude
        ///</summary>
        ///<param name = "lat">
        ///  the latitude of observer in degrees </param>
        ///<param name = "solarDec">
        ///  the declination angle of sun in degrees </param>
        ///<returns> the hour angle of sunset in radians
        ///  TODO: use - calcHourAngleSunrise implementation </returns>
        private static double calcHourAngleSunset(double lat, double solarDec, double zenith)
        {
            double latRad = MathExtensions.ToRadians(lat);
            double sdRad = MathExtensions.ToRadians(solarDec);

            // double HAarg =
            // (Math.cos(MathExtensions.ToRadians(zenith))/(Math.cos(latRad)*Math.cos(sdRad))-Math.tan(latRad)
            // * Math.tan(sdRad));

            double HA =
                (Math.Acos(Math.Cos(MathExtensions.ToRadians(zenith))/(Math.Cos(latRad)*Math.Cos(sdRad)) -
                           Math.Tan(latRad)*Math.Tan(sdRad)));
            return -HA; // in radians
        }

        ///<summary>
        ///  Calculate the Universal Coordinated Time (UTC) of sunrise for the given
        ///  day at the given location on earth
        ///</summary>
        ///<param name = "JD">
        ///  the julian day </param>
        ///<param name = "latitude">
        ///  the latitude of observer in degrees </param>
        ///<param name = "longitude">
        ///  the longitude of observer in degrees </param>
        ///<returns> the time in minutes from zero Z </returns>
        private static double calcSunriseUTC(double JD, double latitude, double longitude, double zenith)
        {
            double t = calcTimeJulianCent(JD);

            // *** Find the time of solar noon at the location, and use
            // that declination. This is better than start of the
            // Julian day

            double noonmin = calcSolNoonUTC(t, longitude);
            double tnoon = calcTimeJulianCent(JD + noonmin/1440.0);

            // *** First pass to approximate sunrise (using solar noon)

            double eqTime = calcEquationOfTime(tnoon);
            double solarDec = calcSunDeclination(tnoon);
            double hourAngle = calcHourAngleSunrise(latitude, solarDec, zenith);

            double delta = longitude - MathExtensions.ToDegree(hourAngle);
            double timeDiff = 4*delta; // in minutes of time
            double timeUTC = 720 + timeDiff - eqTime; // in minutes

            // *** Second pass includes fractional jday in gamma calc

            double newt = calcTimeJulianCent(calcJDFromJulianCent(t) + timeUTC/1440.0);
            eqTime = calcEquationOfTime(newt);
            solarDec = calcSunDeclination(newt);
            hourAngle = calcHourAngleSunrise(latitude, solarDec, zenith);
            delta = longitude - MathExtensions.ToDegree(hourAngle);
            timeDiff = 4*delta;
            timeUTC = 720 + timeDiff - eqTime; // in minutes
            return timeUTC;
        }

        ///<summary>
        ///  calculate the Universal Coordinated Time (UTC) of solar noon for the
        ///  given day at the given location on earth
        ///</summary>
        ///<param name = "t">
        ///  the number of Julian centuries since J2000.0 </param>
        ///<param name = "longitude">
        ///  the longitude of observer in degrees </param>
        ///<returns> the time in minutes from zero Z </returns>
        private static double calcSolNoonUTC(double t, double longitude)
        {
            // First pass uses approximate solar noon to calculate eqtime
            double tnoon = calcTimeJulianCent(calcJDFromJulianCent(t) + longitude/360.0);
            double eqTime = calcEquationOfTime(tnoon);
            double solNoonUTC = 720 + (longitude*4) - eqTime; // min

            double newt = calcTimeJulianCent(calcJDFromJulianCent(t) - 0.5 + solNoonUTC/1440.0);

            eqTime = calcEquationOfTime(newt);
            return 720 + (longitude*4) - eqTime; // min
        }

        ///<summary>
        ///  calculate the Universal Coordinated Time (UTC) of sunset for the given
        ///  day at the given location on earth
        ///</summary>
        ///<param name = "JD">
        ///  the julian day </param>
        ///<param name = "latitude">
        ///  the latitude of observer in degrees </param>
        ///<param name = "longitude"> :
        ///  longitude of observer in degrees </param>
        ///<param name = "zenith"> </param>
        ///<returns> the time in minutes from zero Z </returns>
        private static double calcSunsetUTC(double JD, double latitude, double longitude, double zenith)
        {
            double t = calcTimeJulianCent(JD);

            // *** Find the time of solar noon at the location, and use
            // that declination. This is better than start of the
            // Julian day

            double noonmin = calcSolNoonUTC(t, longitude);
            double tnoon = calcTimeJulianCent(JD + noonmin/1440.0);

            // First calculates sunrise and approx length of day

            double eqTime = calcEquationOfTime(tnoon);
            double solarDec = calcSunDeclination(tnoon);
            double hourAngle = calcHourAngleSunset(latitude, solarDec, zenith);

            double delta = longitude - MathExtensions.ToDegree(hourAngle);
            double timeDiff = 4*delta;
            double timeUTC = 720 + timeDiff - eqTime;

            // first pass used to include fractional day in gamma calc

            double newt = calcTimeJulianCent(calcJDFromJulianCent(t) + timeUTC/1440.0);
            eqTime = calcEquationOfTime(newt);
            solarDec = calcSunDeclination(newt);
            hourAngle = calcHourAngleSunset(latitude, solarDec, zenith);

            delta = longitude - MathExtensions.ToDegree(hourAngle);
            timeDiff = 4*delta;
            return 720 + timeDiff - eqTime; // in minutes
        }
    }
}