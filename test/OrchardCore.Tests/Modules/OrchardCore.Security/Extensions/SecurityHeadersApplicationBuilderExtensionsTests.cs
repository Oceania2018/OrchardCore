using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace OrchardCore.Security.Extensions.Tests
{
    public class SecurityHeadersApplicationBuilderExtensionsTests
    {
        [Fact]
        public void UseSecurityHeadersWithDefaultHeaders()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var applicationBuilder = CreateApplicationBuilder();

            // Act
            applicationBuilder.UseSecurityHeaders();

            applicationBuilder
                .Build()
                .Invoke(context);

            // Assert
            Assert.Equal(ContentTypeOptionsValue.NoSniff, context.Response.Headers[SecurityHeaderNames.XContentTypeOptions]);
            Assert.Equal(SecurityHeaderDefaults.FrameOptions, context.Response.Headers[SecurityHeaderNames.XFrameOptions]);
            Assert.Equal("accelerometer=(), ambient-light-sensor=(), autoplay=(), camera=(), encrypted-media=(), fullscreen=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), midi=(), notifications=(), payment=(), picture-in-picture=(), push=(), speaker=(), sync-xhr=(), usb=(), vibrate=(), vr=()", context.Response.Headers[SecurityHeaderNames.PermissionsPolicy]);
            Assert.Equal(SecurityHeaderDefaults.ReferrerPolicy, context.Response.Headers[SecurityHeaderNames.ReferrerPolicy]);
            Assert.Equal($"max-age={SecurityHeaderDefaults.StrictTransportSecurityOptions.MaxAge.TotalSeconds}; includeSubDomains", context.Response.Headers[SecurityHeaderNames.StrictTransportSecurity]);
        }

        [Fact]
        public void UseSecurityHeadersWithOptions()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var options = new SecurityHeadersOptions
            {
                ContentTypeOptions = new ContentTypeOptionsOptions(),
                FrameOptions = new FrameOptionsOptions { Value = FrameOptionsValue.Deny },
                PermissionsPolicy = new PermissionsPolicyOptions
                {
                    Camera = new CameraPermissionsPolicyOptions { Origin = PermissionsPolicyOriginValue.Self },
                    Microphone = new MicrophonePermissionsPolicyOptions { Origin = PermissionsPolicyOriginValue.Any }
                },
                ReferrerPolicy = new ReferrerPolicyOptions { Value = ReferrerPolicyValue.Origin },
                StrictTransportSecurity = new StrictTransportSecurityOptions { MaxAge = TimeSpan.FromSeconds(60) }
            };
            var applicationBuilder = CreateApplicationBuilder();

            // Act
            applicationBuilder.UseSecurityHeaders(options);

            applicationBuilder
                .Build()
                .Invoke(context);

            // Assert
            Assert.Equal(ContentTypeOptionsValue.NoSniff, context.Response.Headers[SecurityHeaderNames.XContentTypeOptions]);
            Assert.Equal(FrameOptionsValue.Deny, context.Response.Headers[SecurityHeaderNames.XFrameOptions]);
            Assert.Equal("accelerometer=(), ambient-light-sensor=(), autoplay=(), camera=self, encrypted-media=(), fullscreen=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=*, midi=(), notifications=(), payment=(), picture-in-picture=(), push=(), speaker=(), sync-xhr=(), usb=(), vibrate=(), vr=()", context.Response.Headers[SecurityHeaderNames.PermissionsPolicy]);
            Assert.Equal(ReferrerPolicyValue.Origin, context.Response.Headers[SecurityHeaderNames.ReferrerPolicy]);
            Assert.Equal($"max-age=60; includeSubDomains", context.Response.Headers[SecurityHeaderNames.StrictTransportSecurity]);
        }

        [Fact]
        public void UseSecurityHeadersWithBuilderConfiguration()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var applicationBuilder = CreateApplicationBuilder();

            // Act
            applicationBuilder.UseSecurityHeaders(config =>
            {
                config
                    .AddContentTypeOptions()
                    .AddFrameOptions(options => options.WithDeny())
                    .AddPermissionsPolicy(options =>
                    {
                        options
                            .AllowCamera(PermissionsPolicyOriginValue.Self)
                            .AllowMicrophone(PermissionsPolicyOriginValue.Any);
                    })
                    .AddReferrerPolicy(options => options .WithOrigin())
                    .AddStrictTransportSecurity(options => options.WithMaxAge(TimeSpan.FromMinutes(1)));
            });

            applicationBuilder
                .Build()
                .Invoke(context);

            // Assert
            Assert.Equal(ContentTypeOptionsValue.NoSniff, context.Response.Headers[SecurityHeaderNames.XContentTypeOptions]);
            Assert.Equal(FrameOptionsValue.Deny, context.Response.Headers[SecurityHeaderNames.XFrameOptions]);
            Assert.Equal("accelerometer=(), ambient-light-sensor=(), autoplay=(), camera=self, encrypted-media=(), fullscreen=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=*, midi=(), notifications=(), payment=(), picture-in-picture=(), push=(), speaker=(), sync-xhr=(), usb=(), vibrate=(), vr=()", context.Response.Headers[SecurityHeaderNames.PermissionsPolicy]);
            Assert.Equal(ReferrerPolicyValue.Origin, context.Response.Headers[SecurityHeaderNames.ReferrerPolicy]);
            Assert.Equal($"max-age=60; includeSubDomains", context.Response.Headers[SecurityHeaderNames.StrictTransportSecurity]);
        }

        private static IApplicationBuilder CreateApplicationBuilder()
        {
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            return new ApplicationBuilder(serviceProvider);
        }
    }
}