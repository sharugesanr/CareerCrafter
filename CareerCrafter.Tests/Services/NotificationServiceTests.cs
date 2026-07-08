using CareerCrafter.Core.DTOs;
using CareerCrafter.Core.Exceptions;
using CareerCrafter.Core.Models;
using CareerCrafter.Repositories.Interfaces;
using CareerCrafter.Services.Implementations;
using Moq;
using NUnit.Framework;

namespace CareerCrafter.Tests.Services
{
    [TestFixture]
    public class NotificationServiceTests
    {
        private Mock<INotificationRepository> _notificationRepoMock = null!;
        private NotificationService _notificationService = null!;

        [SetUp]
        public void SetUp()
        {
            _notificationRepoMock = new Mock<INotificationRepository>();

            _notificationService = new NotificationService(
                _notificationRepoMock.Object);
        }

        [Test]
        public async Task CreateNotificationAsync_ValidData_AddsNotification()
        {
             
            int userId = 1;
            string message = "Application submitted successfully.";

            _notificationRepoMock
                .Setup(r => r.AddAsync(It.IsAny<Notification>()))
                .Returns(Task.CompletedTask);

            _notificationRepoMock
                .Setup(r => r.SaveChangesAsync())
                .Returns(Task.CompletedTask);

             
            await _notificationService.CreateNotificationAsync(userId, message);

             
            _notificationRepoMock.Verify(r =>
                r.AddAsync(It.Is<Notification>(n =>
                    n.UserId == userId &&
                    n.Message == message &&
                    n.IsRead == false)),
                Times.Once);

            _notificationRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task GetMyNotificationsAsync_ReturnsNotificationDtos()
        {
             
            var createdDate = DateTime.Now;

            var notifications = new List<Notification>
    {
        new Notification
        {
            NotificationId = 1,
            UserId = 1,
            Message = "Welcome",
            IsRead = false,
            CreatedAt = createdDate
        }
    };

            _notificationRepoMock
                .Setup(r => r.GetByUserIdAsync(1))
                .ReturnsAsync(notifications);

             
            var result = await _notificationService.GetMyNotificationsAsync(1);

             
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].NotificationId, Is.EqualTo(1));
            Assert.That(result[0].Message, Is.EqualTo("Welcome"));
            Assert.That(result[0].IsRead, Is.False);
            Assert.That(result[0].CreatedAt, Is.EqualTo(createdDate));
        }

        [Test]
        public async Task MarkAsReadAsync_ValidNotification_UpdatesNotification()
        {
             
            int userId = 1;
            int notificationId = 10;

            var notification = new Notification
            {
                NotificationId = notificationId,
                UserId = userId,
                Message = "Test Notification",
                IsRead = false
            };

            _notificationRepoMock
                .Setup(r => r.GetByIdAsync(notificationId))
                .ReturnsAsync(notification);

            _notificationRepoMock
                .Setup(r => r.UpdateAsync(It.IsAny<Notification>()))
                .Returns(Task.CompletedTask);

            _notificationRepoMock
                .Setup(r => r.SaveChangesAsync())
                .Returns(Task.CompletedTask);

             
            await _notificationService.MarkAsReadAsync(userId, notificationId);

             
            Assert.That(notification.IsRead, Is.True);

            _notificationRepoMock.Verify(r =>
                r.UpdateAsync(It.IsAny<Notification>()),
                Times.Once);

            _notificationRepoMock.Verify(r =>
                r.SaveChangesAsync(),
                Times.Once);
        }

        [Test]
        public void MarkAsReadAsync_NotificationNotFound_ThrowsNotFoundException()
        {
             
            int userId = 1;
            int notificationId = 100;

            _notificationRepoMock
                .Setup(r => r.GetByIdAsync(notificationId))
                .ReturnsAsync((Notification?)null);

            
            var ex = Assert.ThrowsAsync<NotFoundException>(async () =>
                await _notificationService.MarkAsReadAsync(userId, notificationId));

            Assert.That(ex!.Message, Is.EqualTo("Notification not found."));
        }

        [Test]
        public void MarkAsReadAsync_UnauthorizedUser_ThrowsUnauthorizedException()
        {
             
            int loggedInUserId = 1;
            int notificationId = 5;

            var notification = new Notification
            {
                NotificationId = notificationId,
                UserId = 2,
                Message = "Private Notification",
                IsRead = false
            };

            _notificationRepoMock
                .Setup(r => r.GetByIdAsync(notificationId))
                .ReturnsAsync(notification);

            var ex = Assert.ThrowsAsync<UnauthorizedException>(async () =>
                await _notificationService.MarkAsReadAsync(loggedInUserId, notificationId));

            Assert.That(ex!.Message,
                Is.EqualTo("You are not authorized to access this notification."));
        }

        [Test]
        public async Task CreateNotificationAsync_SetsIsReadToFalse()
        {
             
            Notification? addedNotification = null;

            _notificationRepoMock
                .Setup(r => r.AddAsync(It.IsAny<Notification>()))
                .Callback<Notification>(n => addedNotification = n)
                .Returns(Task.CompletedTask);

            _notificationRepoMock
                .Setup(r => r.SaveChangesAsync())
                .Returns(Task.CompletedTask);

             
            await _notificationService.CreateNotificationAsync(1, "Test Notification");

             
            Assert.That(addedNotification, Is.Not.Null);
            Assert.That(addedNotification!.IsRead, Is.False);
        }

        [Test]
        public async Task CreateNotificationAsync_SetsCreatedAt()
        {
             
            Notification? addedNotification = null;

            _notificationRepoMock
                .Setup(r => r.AddAsync(It.IsAny<Notification>()))
                .Callback<Notification>(n => addedNotification = n)
                .Returns(Task.CompletedTask);

            _notificationRepoMock
                .Setup(r => r.SaveChangesAsync())
                .Returns(Task.CompletedTask);

             
            await _notificationService.CreateNotificationAsync(1, "Test Notification");

             
            Assert.That(addedNotification, Is.Not.Null);
            Assert.That(addedNotification!.CreatedAt, Is.Not.Null);
        }

        [Test]
        public async Task GetMyNotificationsAsync_NoNotifications_ReturnsEmptyList()
        {
             
            _notificationRepoMock
                .Setup(r => r.GetByUserIdAsync(1))
                .ReturnsAsync(new List<Notification>());

             
            var result = await _notificationService.GetMyNotificationsAsync(1);

             
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(0));
        }
    }
}