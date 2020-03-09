﻿namespace QuizHut.Web.Areas.Administration.Controllers
{
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using QuizHut.Data.Models;
    using QuizHut.Services.Events;
    using QuizHut.Services.Groups;
    using QuizHut.Services.Users;
    using QuizHut.Web.Filters;
    using QuizHut.Web.ViewModels.Events;
    using QuizHut.Web.ViewModels.Groups;
    using QuizHut.Web.ViewModels.Students;

    public class GroupsController : AdministrationController
    {
        private readonly IGroupsService service;
        private readonly IEventsService eventService;
        private readonly IUsersService userService;
        private readonly UserManager<ApplicationUser> userManager;

        public GroupsController(
            IGroupsService service,
            IEventsService eventService,
            IUsersService userService,
            UserManager<ApplicationUser> userManager)
        {
            this.service = service;
            this.eventService = eventService;
            this.userService = userService;
            this.userManager = userManager;
        }

        public async Task<IActionResult> AllGroupsCreatedByTeacher()
        {
            var userId = this.userManager.GetUserId(this.User);
            var groups = await this.service.GetAllByCreatorIdAsync<GroupListViewModel>(userId);
            var model = new GroupsListAllViewModel() { Groups = groups };
            return this.View(model);
        }

        public IActionResult Create()
        {
            return this.View();
        }

        [HttpPost]
        [ModelStateValidationActionFilterAttribute]
        public async Task<IActionResult> Create(CreateGroupInputViewModel model)
        {
            var userId = this.userManager.GetUserId(this.User);
            var groupId = await this.service.CreateAsync(model.Name, userId);

            return this.RedirectToAction("AssignEvent", new { id = groupId });
        }

        public async Task<IActionResult> AssignEvent(string id)
        {
            var userId = this.userManager.GetUserId(this.User);
            var events = await this.eventService.GetAllByCreatorIdAsync<EventsAssignViewModel>(userId);
            var model = await this.service.GetGroupModelAsync<GroupWithEventsViewModel>(id);
            model.Events = events;

            return this.View(model);
        }

        [HttpPost]
        [ModelStateValidationActionFilterAttribute]
        public async Task<IActionResult> AssignEvent(GroupWithEventsViewModel model)
        {
            var eventsIds = model.Events.Where(x => x.IsAssigned).Select(x => x.Id).ToList();

            if (eventsIds.Count() != 1)
            {
                model.Error = true;
                return this.View(model);
            }

            await this.service.AssignEventsToGroupAsync(model.Id, eventsIds);
            return this.RedirectToAction("AssignStudents", new { id = model.Id });
        }

        public async Task<IActionResult> AssignStudents(string id)
        {
            var userId = this.userManager.GetUserId(this.User);
            var students = await this.userService.GetAllByUserIdAsync<StudentViewModel>(userId);
            var model = new GroupWithStudentsViewModel() { Id = id, Students = students };
            return this.View(model);
        }

        [HttpPost]
        [ModelStateValidationActionFilterAttribute]
        public async Task<IActionResult> AssignStudents(GroupWithStudentsViewModel model)
        {
            var studentsIds = model.Students.Where(x => x.IsAssigned).Select(x => x.Id).ToList();

            if (studentsIds.Count() == 0)
            {
                model.Error = true;
                return this.View(model);
            }

            await this.service.AssignStudentsToGroupAsync(model.Id, studentsIds);
            return this.RedirectToAction("GroupDetails", new { id = model.Id });
        }

        [HttpGet]
        public async Task<IActionResult> GroupDetails(string id)
        {
            var events = await this.eventService.GetAllByGroupIdAsync<EventsAssignViewModel>(id);
            var students = await this.userService.GetAllByGroupIdAsync<StudentViewModel>(id);
            var model = await this.service.GetGroupModelAsync<GroupDetailsViewModel>(id);
            model.Students = students;
            model.Events = events;
            return this.View(model);
        }

        public IActionResult Edit(string id)
        {
            return this.RedirectToAction("GroupDetails", new { id });
        }

        public async Task<IActionResult> Delete(string id)
        {
            await this.service.DeleteAsync(id);
            return this.RedirectToAction("AllGroupsCreatedByTeacher");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteEventFromGroup(string groupId, string eventId)
        {
            await this.service.DeleteEventFromGroupAsync(groupId, eventId);
            return this.RedirectToAction("GroupDetails", new { id = groupId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteStudentFromGroup(string groupId, string studentId)
        {
            await this.service.DeleteStudentFromGroupAsync(groupId, studentId);
            return this.RedirectToAction("GroupDetails", new { id = groupId });
        }

        public async Task<IActionResult> AddNewEvent(string id)
        {
            var userId = this.userManager.GetUserId(this.User);
            var events = await this.eventService.GetAllByCreatorIdAsync<EventsAssignViewModel>(userId, id);
            var model = await this.service.GetGroupModelAsync<GroupWithEventsViewModel>(id);
            model.Events = events;
            return this.View(model);
        }

        [HttpPost]
        [ModelStateValidationActionFilterAttribute]
        public async Task<IActionResult> AddNewEvent(GroupWithEventsViewModel model)
        {
            var eventsIds = model.Events.Where(x => x.IsAssigned).Select(x => x.Id).ToList();

            if (eventsIds.Count() == 0)
            {
                model.Error = true;
                return this.View(model);
            }

            await this.service.AssignEventsToGroupAsync(model.Id, eventsIds);
            return this.RedirectToAction("GroupDetails", new { id = model.Id });
        }

        public async Task<IActionResult> AddStudents(string id)
        {
            var userId = this.userManager.GetUserId(this.User);
            var students = await this.userService.GetAllByUserIdAsync<StudentViewModel>(userId, id);
            var model = new GroupWithStudentsViewModel() { Id = id, Students = students };
            model.Students = students;
            return this.View(model);
        }

        [HttpPost]
        [ModelStateValidationActionFilterAttribute]
        public async Task<IActionResult> AddStudents(GroupWithStudentsViewModel model)
        {
            var studentsIds = model.Students.Where(x => x.IsAssigned).Select(x => x.Id).ToList();

            if (studentsIds.Count() == 0)
            {
                model.Error = true;
                return this.View(model);
            }

            await this.service.AssignStudentsToGroupAsync(model.Id, studentsIds);
            return this.RedirectToAction("GroupDetails", new { id = model.Id });
        }

        public IActionResult EditName(string id, string name)
        {
            var model = new EditGroupNameInputViewModel() { Id = id, Name = name };
            return this.View(model);
        }

        [HttpPost]
        [ModelStateValidationActionFilterAttribute]
        public async Task<IActionResult> EditName(EditGroupNameInputViewModel model)
        {
            await this.service.UpdateNameAsync(model.Id, model.Name);
            return this.RedirectToAction("GroupDetails", new { id = model.Id });
        }
    }
}
