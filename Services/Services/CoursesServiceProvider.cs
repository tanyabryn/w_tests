using System.Collections.Generic;
using System.Linq;
using CoursesAPI.Models;
using CoursesAPI.Services.DataAccess;
using CoursesAPI.Services.Exceptions;
using CoursesAPI.Services.Models.Entities;
using System;

namespace CoursesAPI.Services.Services
{
    public class CoursesServiceProvider
	{
		private readonly IUnitOfWork _uow;

		private readonly IRepository<CourseInstance> _courseInstances;
		private readonly IRepository<TeacherRegistration> _teacherRegistrations;
		private readonly IRepository<CourseTemplate> _courseTemplates; 
		private readonly IRepository<Person> _persons;

		public CoursesServiceProvider(IUnitOfWork uow)
		{
			_uow = uow;

			_courseInstances      = _uow.GetRepository<CourseInstance>();
			_courseTemplates      = _uow.GetRepository<CourseTemplate>();
			_teacherRegistrations = _uow.GetRepository<TeacherRegistration>();
			_persons              = _uow.GetRepository<Person>();
		}

		/// <summary>
		/// You should implement this function, such that all tests will pass.
		/// </summary>
		/// <param name="courseInstanceID">The ID of the course instance which the teacher will be registered to.</param>
		/// <param name="model">The data which indicates which person should be added as a teacher, and in what role.</param>
		/// <returns>Should return basic information about the person.</returns>
		public PersonDTO AddTeacherToCourse(int courseInstanceID, AddTeacherViewModel model)
		{
			var course = _courseInstances.All().SingleOrDefault(x => x.ID == courseInstanceID);
            if(course == null)
            {
                throw new AppObjectNotFoundException();
            }

            var teacher = _persons.All().SingleOrDefault(x => x.SSN == model.SSN);
            if(teacher == null)
            {
                throw new AppObjectNotFoundException();
            }
            var mainTeacher = (from t in _teacherRegistrations.All()
                               where t.CourseInstanceID == courseInstanceID
                               && t.Type == TeacherType.MainTeacher
                               select t).SingleOrDefault();

            if(mainTeacher != null && model.Type == TeacherType.MainTeacher)
            {
                throw new AppValidationException("COURSE_ALREADY_HAS_A_MAIN_TEACHER");
            }

            var teacherInCourse = (from t in _teacherRegistrations.All()
                                   where t.CourseInstanceID == courseInstanceID
                                   && t.SSN == model.SSN
                                   select t).SingleOrDefault();
            
            
            if(teacherInCourse != null)
            {
                throw new AppValidationException("PERSON_ALREADY_REGISTERED_TEACHER_IN_COURSE");
            }

            var addTeacher = new TeacherRegistration { CourseInstanceID = courseInstanceID, SSN = model.SSN, Type = model.Type};
            _teacherRegistrations.Add(addTeacher);

            var person = (from p in _persons.All()
                          where p.SSN == model.SSN
                          select new PersonDTO
                          {
                              SSN = p.SSN,
                              Name = p.Name
                          }).SingleOrDefault();
            _uow.Save();
            return person;
		}

		/// <summary>
		/// You should write tests for this function. You will also need to
		/// modify it, such that it will correctly return the name of the main
		/// teacher of each course.
		/// </summary>
		/// <param name="semester"></param>
		/// <returns></returns>
		public List<CourseInstanceDTO> GetCourseInstancesBySemester(string semester = null)
		{
			if (string.IsNullOrEmpty(semester))
			{
				semester = "20153";
			}

            var courses = (from c in _courseInstances.All()
                           join ct in _courseTemplates.All() on c.CourseID equals ct.CourseID
                           where c.SemesterID == semester
                           select new CourseInstanceDTO
                           {
                               Name = ct.Name,
                               TemplateID = ct.CourseID,
                               CourseInstanceID = c.ID,
                               MainTeacher = (from tr in _teacherRegistrations.All()
                                              join p in _persons.All() on tr.SSN equals p.SSN into ps
                                              where tr.CourseInstanceID == c.ID && tr.Type == TeacherType.MainTeacher
                                              from p in ps.DefaultIfEmpty()
                                              select (p.Name == null ? "" : p.Name)).DefaultIfEmpty("").FirstOrDefault()
				}).ToList();

			return courses;
		}
	}
}
