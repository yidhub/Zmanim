using System;
using System.Collections.Generic;
using System.Text;
using java.util;

namespace ZmanimTests.TestGeneration.TestFormatters
{
    public class DotNetTestFormatter : ITestFormatter
    {
        public DotNetTestFormatter()
        {
            TestMethods = new List<string>();
            TestMethods.Add(@"
        //We can use these test when removing the depenency to Java (IKVM)
        //To make sure that the code stayes the same.

        private ComplexZmanimCalendar calendar;

        [SetUp]
        public void Setup()
        {
            String locationName = ""Lakewood, NJ"";
            double latitude = 40.09596; //Lakewood, NJ
            double longitude = -74.22213; //Lakewood, NJ
            double elevation = 0; //optional elevation
            TimeZone timeZone = TimeZone.getTimeZone(""America/New_York"");
            GeoLocation location = new GeoLocation(locationName, latitude, longitude, elevation, timeZone);
            ComplexZmanimCalendar czc = new ComplexZmanimCalendar(location);

            czc.setCalendar(new GregorianCalendar(2010, 3, 2));
            calendar = czc;
        }
");
        }

        public string ClassName { get; set; }
        public ITestFormatter SetClassName(string name)
        {
            ClassName = name;
            return this;
        }

        public ITestFormatter AddTestMethod(string methodName, string testBody)
        {
            TestMethods.Add(string.Format(@"
        [Test]
        public void Check_{0}()
        {{
            {1}
        }}", methodName, testBody));

            return this;
        }

        public ITestFormatter AddDateTestMethod(string methodName, Date date)
        {
            var calendar = new GregorianCalendar();
            calendar.setTime(date);

            AddTestMethod(methodName,
                string.Format(
                @"var zman = calendar.{0}().ToDateTime();

            Assert.That(zman, Is.EqualTo(
                    new DateTime({1}, {2}, {3}, {4}, {5}, {6})
                ));",
                    methodName,
                    calendar.get(Calendar.YEAR),
                    calendar.get(Calendar.MONTH) + 1,
                    calendar.get(Calendar.DAY_OF_MONTH),
                    calendar.get(Calendar.HOUR_OF_DAY),
                    calendar.get(Calendar.MINUTE),
                    calendar.get(Calendar.SECOND)
                /*,calender.get(Calendar.MILLISECOND)*/
                    ));
            return this;
        }

        public IList<string> TestMethods { get; set; }

        public string BuildTestClass()
        {
            var sb = new StringBuilder();

            foreach (var testMethod in TestMethods)
                sb.AppendLine(testMethod);

            return string.Format(@"
using System;
using java.util;
using net.sourceforge.zmanim;
using net.sourceforge.zmanim.util;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Zmanim.Extensions;
using TimeZone = java.util.TimeZone;

namespace ZmanimTests
{{
    [TestFixture]
    public class {0}
    {{
        {1}
    }}
}}", ClassName, sb);
        }
    }
}