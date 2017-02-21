using System;
using System.Collections.Generic;
using System.Linq;
using CoursesAPI.Models;
using CoursesAPI.Services.Exceptions;
using CoursesAPI.Services.Models.Entities;
using CoursesAPI.Services.Services;
using CoursesAPI.Tests.MockObjects;
using Xunit;

namespace CoursesAPI.Tests.Services
{
    public class CourseServicesTests
    {
        private MockUnitOfWork<MockDataContext> _mockUnitOfWork;
        private CoursesServiceProvider _service;
        private List<TeacherRegistration> _teacherRegistrations;

        private const string SSN_DABS = "1203735289";
        private const string SSN_GUNNA = "1234567890";
        private const string SSN_Hrafn = "1245367809";
        private const string INVALID_SSN = "9876543210";

        private const string NAME_GUNNA = "Guðrún Guðmundsdóttir";
        private const string NAME_HRAFN = "Hrafn Loftsson";

        private const int COURSEID_VEFT_20153 = 1337;
        private const int COURSEID_VEFT_20163 = 1338;
        private const int COURSEID_VEFT_20141 = 1339;
        private const int COURSEID_PROG_20153 = 1340;
        private const int COURSEID_PROG_20163 = 1341;
        private const int COURSEID_THYD_20163 = 1342;
        private const int COURSEID_PROG_20143 = 1343;
        private const int INVALID_COURSEID = 9999;
       

        public CourseServicesTests()
        {
            _mockUnitOfWork = new MockUnitOfWork<MockDataContext>();

            #region Persons
            var persons = new List<Person>
            {
				// Of course I'm the first person,
				// did you expect anything else?
				new Person
                {
                    ID    = 1,
                    Name  = "Daníel B. Sigurgeirsson",
                    SSN   = SSN_DABS,
                    Email = "dabs@ru.is"
                },
                new Person
                {
                    ID    = 2,
                    Name  = NAME_GUNNA,
                    SSN   = SSN_GUNNA,
                    Email = "gunna@ru.is"
                },
                new Person
                {
                    ID = 3,
                    Name = NAME_HRAFN,
                    SSN = SSN_Hrafn,
                    Email = "hrafn@ru.is"
                }
            };
            #endregion

            #region Course templates

            var courseTemplates = new List<CourseTemplate>
            {
                new CourseTemplate
                {
                    CourseID    = "T-514-VEFT",
                    Description = "Í þessum áfanga verður fjallað um vefþj...",
                    Name        = "Vefþjónustur"
                },
                new CourseTemplate
                {
                    CourseID    = "T-111-PROG",
                    Description = "Í þessum áfanga verður fjallað um forritun...",
                    Name        = "Forritun"
                },
                new CourseTemplate
                {
                    CourseID    = "T-603-THYD",
                    Description = "Í þessum áfanga verður fjallað um þýðendur...",
                    Name        = "Þýðendur"
                }
            };
            #endregion

            #region Courses
            var courses = new List<CourseInstance>
            {
                new CourseInstance
                {
                    ID         = COURSEID_VEFT_20153,
                    CourseID   = "T-514-VEFT",
                    SemesterID = "20153"
                },
                new CourseInstance
                {
                    ID         = COURSEID_VEFT_20163,
                    CourseID   = "T-514-VEFT",
                    SemesterID = "20163"
                },
                new CourseInstance
                {
                    ID         = COURSEID_PROG_20153,
                    CourseID   = "T-111-PROG",
                    SemesterID = "20153"
                },
                new CourseInstance
                {
                    ID          = COURSEID_PROG_20143,
                    CourseID    = "T-111-PROG",
                    SemesterID  = "20143"
                },
                new CourseInstance
                {
                    ID         = COURSEID_PROG_20163,
                    CourseID   = "T-111-PROG",
                    SemesterID = "20163"
                },
                new CourseInstance
                {
                    ID          = COURSEID_THYD_20163,
                    CourseID    = "T-603-THYD",
                    SemesterID  = "20163"
                }
            };
            #endregion

            #region Teacher registrations
            _teacherRegistrations = new List<TeacherRegistration>
            {
                new TeacherRegistration
                {
                    ID               = 101,
                    CourseInstanceID = COURSEID_VEFT_20153,
                    SSN              = SSN_DABS,
                    Type             = TeacherType.MainTeacher
                },
                new TeacherRegistration
                {
                    ID               = 102,
                    CourseInstanceID = COURSEID_PROG_20163,
                    SSN              = SSN_GUNNA,
                    Type             = TeacherType.AssistantTeacher
                },
                new TeacherRegistration
                {
                    ID               = 103,
                    CourseInstanceID = COURSEID_PROG_20143,
                    SSN              = SSN_Hrafn,
                    Type             = TeacherType.MainTeacher
                },
                new TeacherRegistration
                {
                    ID               = 104,
                    CourseInstanceID = COURSEID_PROG_20143,
                    SSN              = SSN_GUNNA,
                    Type             = TeacherType.AssistantTeacher
                }
                

            };
            #endregion

            _mockUnitOfWork.SetRepositoryData(persons);
            _mockUnitOfWork.SetRepositoryData(courseTemplates);
            _mockUnitOfWork.SetRepositoryData(courses);
            _mockUnitOfWork.SetRepositoryData(_teacherRegistrations);

            // TODO: this would be the correct place to add 
            // more mock data to the mockUnitOfWork!

            _service = new CoursesServiceProvider(_mockUnitOfWork);
        }

        #region GetCoursesBySemester
        /// <summary>
        /// Checks if an empty list is returned when no courses have been listed
        /// </summary>
        [Fact]
        public void GetCoursesBySemester_ReturnsEmptyListWhenNoDataDefined()
        {
            // Arrange:
            _mockUnitOfWork.SetRepositoryData(new List<CourseInstance>());

            // Act:
            var courses = _service.GetCourseInstancesBySemester();
            // Assert:
            Assert.Empty(courses);
            // Assert.True(true);
        }

        /// <summary>
        /// Checks if all courses taught on a default semester is returned
        /// and if the Name and TemplateID are correct
        /// </summary>
        [Fact]
        public void GetCoursesBySemester_ReturnsAllCoursesOnADefaultSemester()
        {
            // Arrange:
            // Act:
            var courses = _service.GetCourseInstancesBySemester();
            var count = courses.Count();
            var course1 = courses.SingleOrDefault(x => x.CourseInstanceID == COURSEID_PROG_20153);
            var course2 = courses.SingleOrDefault(x => x.CourseInstanceID == COURSEID_VEFT_20153);

            // Assert:
            Assert.Equal(2, count);
            Assert.NotNull(course1);
            Assert.NotNull(course2);
            Assert.Equal("Forritun", course1.Name);
            Assert.Equal("Vefþjónustur", course2.Name);
            Assert.Equal("T-111-PROG", course1.TemplateID);
            Assert.Equal("T-514-VEFT", course2.TemplateID);
        }

        /// <summary>
        /// Checks if all courses taught on semester 20163 are returned, three courses
        /// should be returned. Also checks if Name and TemplateID are correct.
        /// </summary>
        [Fact]
        public void GetCoursesBySemester_ReturnsAllCoursesFromSemester20163()
        {
            // Arrange:

            // Act:
            var courses = _service.GetCourseInstancesBySemester("20163");
            var count = courses.Count();
            var course1 = courses.SingleOrDefault(x => x.CourseInstanceID == COURSEID_VEFT_20163);
            var course2 = courses.SingleOrDefault(x => x.CourseInstanceID == COURSEID_PROG_20163);
            var course3 = courses.SingleOrDefault(x => x.CourseInstanceID == COURSEID_THYD_20163);

            // Assert:
            Assert.Equal(3, count);

            Assert.NotNull(course1);
            Assert.NotNull(course2);
            Assert.NotNull(course3);

            Assert.Equal("Vefþjónustur", course1.Name);
            Assert.Equal("Forritun", course2.Name);
            Assert.Equal("Þýðendur", course3.Name);

            Assert.Equal("T-514-VEFT", course1.TemplateID);
            Assert.Equal("T-111-PROG", course2.TemplateID);
            Assert.Equal("T-603-THYD", course3.TemplateID);
        }

        /// <summary>
        /// Checks if the main teacher of a course, that only has one teacher that is a main 
        /// teacher, is returned.
        /// </summary>
        [Fact]
        public void GetCoursesBySemester_ReturnsMainTeacher()
        {
            // Arrange:

            // Act:
            var courses = _service.GetCourseInstancesBySemester("20153");
            var course1 = courses.SingleOrDefault(x => x.CourseInstanceID == COURSEID_VEFT_20153);

            // Assert:
            Assert.Equal("Daníel B. Sigurgeirsson", course1.MainTeacher);
        }

        /// <summary>
        /// Checks if an empty string for the name of the teacher of a course, that has no teachers
        /// listed, is returned.
        /// </summary>
        [Fact]
        public void GetCoursesBySemester_ReturnsEmptyStringForTeacher()
        {
            // Arrange:

            // Act:
            var courses = _service.GetCourseInstancesBySemester("20153");
            var course1 = courses.SingleOrDefault(x => x.CourseInstanceID == COURSEID_PROG_20153);

            // Assert:
            Assert.Equal("", course1.MainTeacher);
        }

        /// <summary>
        /// Checks if an empty string for the name of the teacher of a course, that has only
        /// assistant teachers listed, is returned.
        /// </summary>
        [Fact]
        public void GetCoursesBySemester_ReturnsEmptyStringForTeacherAndNotTA()
        {
            // Arrange:

            // Act:
            var courses = _service.GetCourseInstancesBySemester("20163");
            var course1 = courses.SingleOrDefault(x => x.CourseInstanceID == COURSEID_PROG_20163);

            // Assert:
            Assert.Equal("", course1.MainTeacher);
        }
        /// <summary>
        /// Checks if the main teacher of a course, that only has both main teacher and
        /// assistant teacher, is returned
        /// </summary>
        [Fact]
        public void GetCoursesBySemester_ReturnsMainTeacherAndNotTA()
        {
            // Arrange:

            // Act:
            var courses = _service.GetCourseInstancesBySemester("20143");
            var course1 = courses.SingleOrDefault(x => x.CourseInstanceID == COURSEID_PROG_20143);

            // Assert:
            Assert.Equal(NAME_HRAFN, course1.MainTeacher);
        }

        #endregion

        #region AddTeacher

        /// <summary>
        /// Adds a main teacher to a course which doesn't have a
        /// main teacher defined already (see test data defined above).
        /// </summary>
        [Fact]
		public void AddTeacher_WithValidTeacherAndCourse()
		{
			// Arrange:
			var model = new AddTeacherViewModel
			{
				SSN  = SSN_GUNNA,
				Type = TeacherType.MainTeacher
			};
			var prevCount = _teacherRegistrations.Count;
			// Note: the method uses test data defined in [TestInitialize]

			// Act:
			var dto = _service.AddTeacherToCourse(COURSEID_VEFT_20163, model);

			// Assert:

			// Check that the dto object is correctly populated:
			Assert.Equal(SSN_GUNNA, dto.SSN);
			Assert.Equal(NAME_GUNNA, dto.Name);

			// Ensure that a new entity object has been created:
			var currentCount = _teacherRegistrations.Count;
			Assert.Equal(prevCount + 1, currentCount);

			// Get access to the entity object and assert that
			// the properties have been set:
			var newEntity = _teacherRegistrations.Last();
			Assert.Equal(COURSEID_VEFT_20163, newEntity.CourseInstanceID);
			Assert.Equal(SSN_GUNNA, newEntity.SSN);
			Assert.Equal(TeacherType.MainTeacher, newEntity.Type);

			// Ensure that the Unit Of Work object has been instructed
			// to save the new entity object:
			Assert.True(_mockUnitOfWork.GetSaveCallCount() > 0);
		}

		[Fact]
//		[ExpectedException(typeof(AppObjectNotFoundException))]
		public void AddTeacher_InvalidCourse()
		{
			// Arrange:
			var model = new AddTeacherViewModel
			{
				SSN  = SSN_GUNNA,
				Type = TeacherType.AssistantTeacher
			};
			// Note: the method uses test data defined in [TestInitialize]

			// Act:
			Assert.Throws<AppObjectNotFoundException>( () => _service.AddTeacherToCourse(INVALID_COURSEID, model) );
		}

		/// <summary>
		/// Ensure it is not possible to add a person as a teacher
		/// when that person is not registered in the system.
		/// </summary>
		[Fact]
//		[ExpectedException(typeof (AppObjectNotFoundException))]
		public void AddTeacher_InvalidTeacher()
		{
			// Arrange:
			var model = new AddTeacherViewModel
			{
				SSN  = INVALID_SSN,
				Type = TeacherType.MainTeacher
			};
			// Note: the method uses test data defined in [TestInitialize]

			// Act:
			Assert.Throws<AppObjectNotFoundException>( () => _service.AddTeacherToCourse(COURSEID_VEFT_20153, model));
		}

		/// <summary>
		/// In this test, we test that it is not possible to
		/// add another main teacher to a course, if one is already
		/// defined.
		/// </summary>
		[Fact]
		//[ExpectedExceptionWithMessage(typeof (AppValidationException), "COURSE_ALREADY_HAS_A_MAIN_TEACHER")]
		public void AddTeacher_AlreadyWithMainTeacher()
		{
			// Arrange:
			var model = new AddTeacherViewModel
			{
				SSN  = SSN_GUNNA,
				Type = TeacherType.MainTeacher
			};
			// Note: the method uses test data defined in [TestInitialize]

			// Act:
			Exception ex = Assert.Throws<AppValidationException>( () => _service.AddTeacherToCourse(COURSEID_VEFT_20153, model));
			Assert.Equal(ex.Message, "COURSE_ALREADY_HAS_A_MAIN_TEACHER");
		}

		/// <summary>
		/// In this test, we ensure that a person cannot be added as a
		/// teacher in a course, if that person is already registered
		/// as a teacher in the given course.
		/// </summary>
		[Fact]
		// [ExpectedExceptionWithMessage(typeof (AppValidationException), "PERSON_ALREADY_REGISTERED_TEACHER_IN_COURSE")]
		public void AddTeacher_PersonAlreadyRegisteredAsTeacherInCourse()
		{
			// Arrange:
			var model = new AddTeacherViewModel
			{
				SSN  = SSN_DABS,
				Type = TeacherType.AssistantTeacher
			};
			// Note: the method uses test data defined in [TestInitialize]

			// Act:
			Exception ex = Assert.Throws<AppValidationException>( () => _service.AddTeacherToCourse(COURSEID_VEFT_20153, model));
			Assert.Equal(ex.Message, "PERSON_ALREADY_REGISTERED_TEACHER_IN_COURSE");
		}

		#endregion
	}
}
