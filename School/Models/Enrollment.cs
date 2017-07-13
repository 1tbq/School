using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace School.Models
{
    public enum Grade
        {
            A, B, C, D, F
        }

        public class Enrollment
        {
            public int EnrollmentID { get; set; }
            public int CourseID { get; set; }
            public int StudentID { get; set; }
        //The question mark after the Grade type declaration indicates that the Grade property is nullable. 
        //A grade that's null is different from a zero grade -- null means a grade isn't known or hasn't been assigned yet.
        public Grade? Grade { get; set; }

            public Course Course { get; set; }
            public Student Student { get; set; }
        }
   
}
