using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CourseAPI.Models.DTO;
using CourseAPI.Services;
using CoursesAPI.Models;

namespace CourseAPI.Controllers
{
    [Route("api/courses")]
    public class CoursesController : Controller
    {
        private readonly ICoursesService _service;


        public CoursesController(ICoursesService service)
        {
            _service = service;
        }

        /// <summary>
        /// This method returns all courses that are taught in a given semester, if the URL does not contain
        /// a semester a default semester is returned, if there are no courses listed
        /// an empty list is returned.
        /// Returns status code 200
        /// </summary> 
        // GET api/courses
        [HttpGet]
        public IActionResult GetCoursesOnSemester(string semester = null)
        {
            var result = _service.GetCoursesBySemester(semester);
            return Ok(result);

        }

        /// <summary>
        /// This method returns a course with the requested ID, a detailed information about the course and status code 200, 
        /// if there is no course with the given ID then status code 404 is returned
        /// </summary>
        // GET api/courses/id
        [HttpGet]
        [Route("{id:int}", Name = "GetCourse")]
        public IActionResult GetCourseByID(int id)

        {
            try
            {
                var result = _service.GetCourseByID(id);
                return Ok(result);

            }
            catch (AppObjectNotFoundException ex)
            {
                return NotFound();
            }

        }

        /// <summary>
        /// This method creates a new course and returns status code 201. If the course does not have
        /// a valid Template then status code 404 is returned If required information about the course
        /// is missing or in incorrect form then status code 412 is returned.
        /// </summary>
        // POST api/courses
        [HttpPost]
        public IActionResult CreateCourse([FromBody] CreateCourseViewModel newCourse)
        {
            if (newCourse != null && ModelState.IsValid)
            {
                try
                {
                    var result = _service.CreateCourse(newCourse);
                    int ID = result.Item1;
                    var location = Url.Link("GetCourse", new { id = ID });
                    return Created(location, result.Item2);

                }

                catch (AppObjectNotFoundException ex)
                {
                    return NotFound();
                }
            }
            return StatusCode(412);
        }

        /// <summary>
        /// This method returns a list of students in a course with reqested ID and status code 200, 
        /// if there is no course with the requested id then status code 404 is returned.
        /// </summary>
        // GET api/courses/id/students
        [HttpGet]
        [Route("{id:int}/students", Name = "GetStudentsOfCourse")]
        public IActionResult GetStudentsInCourse(int id)
        {
            try
            {
                var result = _service.GetStudentsInCourse(id);
                return Ok(result);
            }
            catch (AppObjectNotFoundException ex)
            {
                return NotFound();
            }
            
        }

        /// <summary>
        /// This method returns a list of students that are on the waiting list for a course 
        /// with reqested ID and status code 200, 
        /// if there is no course with the requested id then status code 404 is returned.
        /// </summary>
        // GET api/courses/id/waitinglist
        [HttpGet]
        [Route("{id:int}/waitinglist", Name = "GetWaitinglist")]
        public IActionResult GetWaitinglistInCourse(int id)
        {
            try
            {
                var result = _service.GetStudentsOnWaitingList(id);
                return Ok(result);
            }
            catch (AppObjectNotFoundException ex)
            {
                return NotFound();
            }

        }

        /// <summary>
        /// This method adds a new students to a waiting list of a course and returns status code 201. 
        /// If the student is not listed in the school or the course with the given id is does not 
        /// exist then status code 404 is returned. If the student is already in the course or 
        /// already on the waiting list status code 412 is returned.
        /// If required information about the student is missing or in incorrect form then status code 412 is returned.
        /// </summary>
        //POST api/courses/id/waitinglist
        [HttpPost]
        [Route("{id:int}/waitinglist")]
        public IActionResult AddStudentToWaitingList(int id, [FromBody] StudentViewModel newStudent)
        {
            if (newStudent != null && ModelState.IsValid)
            {
                try
                {
                    var result = _service.AddStudentToWaitingList(id, newStudent);
                    var location = Url.Link("GetWaitinglist", new { id = id });
                    return Ok();

                }

                catch (AppObjectNotFoundException ex)
                {
                    return NotFound();
                }

                catch (AppDataPreconditionFailedException ex)
                {
                    return StatusCode(412);
                }
            }
            return StatusCode(412);

        }

        /// <summary>
        /// This method adds a new students to course and returns status code 201. If the student is not listed in the school
        /// or the course with the given id is does not exist then status code 404 is returned.
        /// If the student is already in the course status code 400 is returned.
        /// If required information about the student is missing or in incorrect form then status code 412 is returned.
        /// </summary>
        //POST api/courses/id/students
        [HttpPost]
       [Route("{id:int}/students")]
        public IActionResult AddStudentToCourse(int id, [FromBody] StudentViewModel newStudent)
        {
            if (newStudent != null && ModelState.IsValid)
            {
                try
                {
                    var result = _service.AddStudentToCourse(id, newStudent);
                    var location = Url.Link("GetStudentsOfCourse", new { id = id });
                    return Created(location, result);

                }

                catch (AppObjectNotFoundException ex)
                {
                    return NotFound();
                }
                catch (AppObjectBadRequestException ex)
                {
                    return BadRequest();
                }
                catch (AppDataPreconditionFailedException ex)
                {
                    return StatusCode(412);
                }
            }
            return StatusCode(412);
        }

        /// <summary>
        /// This method deletes a students from a course with the requested ID and returns status code 204. 
        /// If the course is not listed or the student is not in the course then status code 404 is returned.
        /// </summary>
        // DELETE api/courses/id/students/SSN
        [HttpDelete("{id:int}/students/{SSN}")]
        public IActionResult DeleteStudentFromCourse(int id , string SSN)
        {
            try
            {
                _service.DeleteStudentFromCourse(id, SSN);
            }
            catch (AppObjectNotFoundException ex)
            {
                return NotFound();
            }
            return NoContent();
        }

        /// <summary>
        /// This method updates an existing course with the requested ID but only the starting date and end date of the course
        /// returns status code 200, if the course does not exist then status code 404 is returned.
        /// If the input is not in the right form status code 412 is returned. 
        /// </summary>
        // PUT api/courses/id
        [HttpPut("{id:int}")]
        public IActionResult UpdateCourse(int id, [FromBody] CourseViewModel updated)
        {
            if(updated != null && ModelState.IsValid)
            {
                try
                {
                    var result = _service.UpdateCourse(id, updated);
                    return Ok(result);
                }
                catch(AppObjectNotFoundException ex)
                {
                    return NotFound();
                }

            }
            return StatusCode(412);

        }

        /// <summary>
        /// This method deletes a course with the requested ID and returns status code 204. If the course is not listed
        /// then status code 404 is returned.
        /// </summary>
        // DELETE api/courses/id
        [HttpDelete("{id:int}")]
        public IActionResult DeleteCourse(int id)
        {
            try
            {
                _service.DeleteCourse(id);
            }
            catch(AppObjectNotFoundException ex)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}
