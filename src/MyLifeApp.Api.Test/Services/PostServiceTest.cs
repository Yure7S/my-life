using Xunit;
using FakeItEasy;
using FluentAssertions;
using MyLifeApp.Application.Services;
using MyLifeApp.Application.Interfaces.Repositories;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Identity.Infrastructure.Models;
using MyLifeApp.Application.Interfaces.Services;
using MyLifeApp.Application.Dtos.Responses.Post;
using MyLifeApp.Domain.Entities;
using Profile = MyLifeApp.Domain.Entities.Profile;
using MyLifeApp.Application.Dtos.Responses.Profile;
using MyLifeApp.Application.Dtos.Requests.Post;
using MyLifeApp.Application.Dtos.Responses;

namespace MyLifeApp.Api.Test
{
    public class PostServiceTest
    {
        private readonly IPostService _postService;
        private readonly IPostRepository _postRepository;
        private readonly IProfileRepository _profileRepository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _context;
        private readonly UserManager<User> _manager;
        private readonly IAuthenticatedProfileService _authenticatedProfileService;

        public PostServiceTest()
        {
            _profileRepository = A.Fake<IProfileRepository>();
            _postRepository = A.Fake<IPostRepository>();
            _mapper = A.Fake<IMapper>();
            _context = A.Fake<IHttpContextAccessor>();
            _manager = A.Fake<UserManager<User>>();
            _authenticatedProfileService = A.Fake<IAuthenticatedProfileService>();
            _postService = new PostService(_postRepository, _profileRepository, _mapper, _context, _manager, _authenticatedProfileService);
        }

        [Fact]
        public async Task GetPublicPostsAsync_GetAllPublicPosts_ReturnSuccess()
        {
            // Arrange
            var posts = A.Fake<ICollection<Post>>();
            var getPostsResponse = A.Fake<ICollection<GetPostsResponse>>();

            GetAllPostsResponse response = new()
            {
                Posts = getPostsResponse,
                Message = "Success",
                IsSuccess = true,
                StatusCode = 200
            };

            A.CallTo(() => _postRepository.GetPublicPostsAsync()).Returns(Task.FromResult(posts));
            A.CallTo(() => _mapper.Map<ICollection<GetPostsResponse>>(posts)).Returns(getPostsResponse);

            // Act
            var result = await _postService.GetPublicPostsAsync();

            // Assert
            result.Should().BeEquivalentTo(response);
            result.StatusCode.Should().Be(200);
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task GetPostByIdAsync_ExistentPost_ReturnsSuccess()
        {
            // Arrange
            var post = A.Fake<Post>();
            var profile = A.Fake<Profile>();
            var user = A.Fake<User>();

            profile.User = user;
            post.Profile = profile;

            var postDetail = A.Fake<DetailPostResponse>();
            var profileMapper = A.Fake<GetProfileResponse>();

            DetailPostResponse response = new()
            {
                Title = post.Title,
                Description = post.Description,
                Profile = profileMapper,
                Message = "Success",
                IsSuccess = true,
                StatusCode = 200
            };

            A.CallTo(() => _postRepository.PostExistsAsync(post.Id)).Returns(true);
            A.CallTo(() => _postRepository.GetPostDetailsAsync(post.Id)).Returns(Task.FromResult(post));
            A.CallTo(() => _mapper.Map<DetailPostResponse>(post)).Returns(postDetail);
            A.CallTo(() => _mapper.Map<GetProfileResponse>(profile)).Returns(profileMapper);

            // Act
            var result = await _postService.GetPostByIdAsync(post.Id);

            // Assert
            result.Should().BeEquivalentTo(response);
            result.StatusCode.Should().Be(200);
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task GetPostByIdAsync_InexistentPost_ReturnsError()
        {
            // Arrange
            Guid inexistentPostGuid = Guid.NewGuid();

            DetailPostResponse response = new()
            {
                Message = "Post not found",
                IsSuccess = false,
                StatusCode = 404
            };

            A.CallTo(() => _postRepository.PostExistsAsync(inexistentPostGuid)).Returns(false);

            // Act
            var result = await _postService.GetPostByIdAsync(inexistentPostGuid);

            result.Should().BeEquivalentTo(response);
            result.StatusCode.Should().Be(404);
            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task CreatePostAsync_ValidPost_ReturnsSuccess()
        {
            // Arrange
            var profile = A.Fake<Profile>();
            var user = A.Fake<User>();
            profile.User = user;

            CreatePostRequest request = new()
            {
                Title = "Testing",
                Description = "Testing description",
                IsPrivate = false
            };

            BaseResponse response = new()
            {
                Message = "Post successfuly created.",
                IsSuccess = true,
                StatusCode = 201
            };

            var post = A.Fake<Post>();
            post.Profile = profile;

            A.CallTo(() => _authenticatedProfileService.GetAuthenticatedProfile()).Returns(Task.FromResult(profile));
            A.CallTo(() => _mapper.Map<Post>(request)).Returns(post);
            A.CallTo(() => _postRepository.CreateAsync(post)).Returns(Task.FromResult(post));

            // Act
            var result = await _postService.CreatePostAsync(request);

            // Assert
            result.Should().BeEquivalentTo(response);
            result.StatusCode.Should().Be(201);
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task UpdatePostAsync_ValidAndExistentPost_ReturnsSuccess()
        {
            // Arrange
            var post = A.Fake<Post>();
            var profile = A.Fake<Profile>();
            var user = A.Fake<User>();

            profile.User = user;
            post.Profile = profile;

            UpdatePostRequest request = new()
            {
                Title = "Testing title",
                Description = "Testing update description",
                IsPrivate = false,
            };

            BaseResponse response = new()
            {
                Message = "Post Successfuly Updated",
                IsSuccess = true,
                StatusCode = 200
            };

            A.CallTo(() => _authenticatedProfileService.GetAuthenticatedProfile()).Returns(Task.FromResult(profile));
            A.CallTo(() => _postRepository.PostExistsAsync(post.Id)).Returns(Task.FromResult(true));
            A.CallTo(() => _postRepository.GetByIdAsync(post.Id)).Returns(Task.FromResult(post));
            A.CallTo(() => _mapper.Map<Post>(request)).Returns(post);
            A.CallTo(() => _postRepository.SaveAsync());

            // Act
            var result = await _postService.UpdatePostAsync(post.Id, request);

            // Assert
            result.Should().BeEquivalentTo(response);
            result.StatusCode.Should().Be(200);
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task UpdatePostAsync_InexistentPost_ReturnsError()
        {
            // Arrange
            Guid inexistentPostGuid = Guid.NewGuid();

            UpdatePostRequest request = new()
            {
                Title = "Testing title",
                Description = "Testing update description",
                IsPrivate = false,
            };

            BaseResponse response = new()
            {
                Message = "Post not found",
                IsSuccess = false,
                StatusCode = 404
            };

            A.CallTo(() => _postRepository.PostExistsAsync(inexistentPostGuid)).Returns(Task.FromResult(false));

            // Act
            var result = await _postService.UpdatePostAsync(inexistentPostGuid, request);

            // Assert
            result.Should().BeEquivalentTo(response);
            result.StatusCode.Should().Be(404);
            result.IsSuccess.Should().BeTrue();
        }

        // ToDo => Implement this test
        // [Fact]
        // public async Task UpdatePostAsync_UpdateWithNotPostCreator_ReturnsError()
        // {
        //     // Arrange
        //     var profile = A.Fake<Profile>();
        //     var user = A.Fake<User>();
        //     profile.User = user;

        //     var anotherProfile = A.Fake<Profile>();
        //     var antotherUser = A.Fake<User>();
        //     anotherProfile.User = antotherUser;

        //     var post = A.Fake<Post>();
        //     post.Profile = profile;

        //     UpdatePostRequest request = new()
        //     {
        //         Title = "Testing title",
        //         Description = "Testing update description",
        //         IsPrivate = false,
        //     };

        //     BaseResponse response = new()
        //     {
        //         Message = "Only post creator can update the post.",
        //         IsSuccess = false,
        //         StatusCode = 400
        //     };
            
        //     A.CallTo(() => _authenticatedProfileService.GetAuthenticatedProfile()).Returns(Task.FromResult(profile));
        //     // Act
        // }
    }
}