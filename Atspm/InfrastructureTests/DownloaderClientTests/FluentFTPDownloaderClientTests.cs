#region license
// Copyright 2026 Utah Departement of Transportation
// for InfrastructureTests - Utah.Udot.Atspm.InfrastructureTests.DownloaderClientTests/FluentFTPDownloaderClientTests.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using FluentFTP;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Utah.Udot.Atspm.Infrastructure.Services.DownloaderClients;
using Xunit.Abstractions;

namespace Utah.Udot.Atspm.InfrastructureTests.DownloaderClientTests
{
    public class FluentFTPDownloaderClientTests : DownloaderClientTestsBase<IAsyncFtpClient>
    {
        public FluentFTPDownloaderClientTests(ITestOutputHelper output) : base(output) { }

        #region MyRegion

        public override async Task ConnectAsyncSucceeded()
        {
            var client = new Mock<IAsyncFtpClient>();
            var config = new Mock<FtpConfig>();

            client.SetupAllProperties();

            client.Setup(s => s.Connect(It.IsAny<CancellationToken>()))
                .Returns(() =>
                {
                    client.SetupGet(p => p.IsConnected).Returns(true);
                    return Task.CompletedTask;
                });

            Client = client.Object;
            Client.Config = config.Object;

            Sut = new FluentFTPDownloaderClient(Client);

            await base.ConnectAsyncSucceeded();
        }

        public override async Task ConnectAsyncNullConnection()
        {
            var client = new Mock<IAsyncFtpClient>();
            var config = new Mock<FtpConfig>();

            client.SetupAllProperties();

            client.Setup(s => s.Connect(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            Client = client.Object;
            Client.Config = config.Object;

            Sut = new FluentFTPDownloaderClient(Client);

            await base.ConnectAsyncNullConnection();
        }

        public override async Task ConnectAsyncNullCredentials()
        {
            var client = new Mock<IAsyncFtpClient>();
            var config = new Mock<FtpConfig>();

            client.SetupAllProperties();

            client.Setup(s => s.Connect(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            Client = client.Object;
            Client.Config = config.Object;

            Sut = new FluentFTPDownloaderClient(Client);

            await base.ConnectAsyncNullCredentials();
        }

        public override async Task ConnectAsyncControllerConnectionException()
        {
            var client = new Mock<IAsyncFtpClient>();
            var config = new Mock<FtpConfig>();

            client.Setup(s => s.Connect(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("FTP connection failed"));
            client.SetupGet(s => s.Config).Returns(config.Object);

            Sut = new FluentFTPDownloaderClient(client.Object);

            await base.ConnectAsyncControllerConnectionException();
        }

        public override async Task ConnectAsyncOperationCanceledException()
        {
            Sut = new FluentFTPDownloaderClient();

            await base.ConnectAsyncOperationCanceledException();
        }

        #endregion

        #region DeleteResource

        public override async Task DeleteResourceAsyncSucceeded()
        {
            var client = new Mock<IAsyncFtpClient>();

            client.SetupGet(p => p.IsConnected).Returns(true);

            Sut = new FluentFTPDownloaderClient(client.Object);

            var uri = new UriBuilder()
            {
                Scheme = Uri.UriSchemeFtp,
                Host = "127.0.0.1",
                Port = 21,
                Path = "/a/b/c.txt"
            }.Uri;

            await Sut.DeleteResourceAsync(uri);
        }

        public override Task DeleteResourceAsyncNullResource()
        {
            var client = new Mock<IAsyncFtpClient>();

            client.SetupGet(p => p.IsConnected).Returns(true);

            Sut = new FluentFTPDownloaderClient(client.Object);

            return base.DeleteResourceAsyncNullResource();
        }

        public override Task DeleteResourceAsyncInvalidResource()
        {
            var client = new Mock<IAsyncFtpClient>();

            client.SetupGet(p => p.IsConnected).Returns(true);

            Sut = new FluentFTPDownloaderClient(client.Object);

            return base.DeleteResourceAsyncInvalidResource();
        }

        public override async Task DeleteResourceAsyncNotConnected()
        {
            var client = new Mock<IAsyncFtpClient>();

            Sut = new FluentFTPDownloaderClient(client.Object);

            await base.DeleteResourceAsyncNotConnected();
        }

        public override async Task DeleteResourceAsyncControllerDeleteResourceException()
        {
            var client = new Mock<IAsyncFtpClient>();

            client.SetupGet(p => p.IsConnected).Returns(true);

            client.Setup(s => s.DeleteFile("", default)).Throws<Exception>();

            Sut = new FluentFTPDownloaderClient(client.Object);

            await base.DeleteResourceAsyncControllerDeleteResourceException();
        }

        public override async Task DeleteResourceAsyncOperationCanceledException()
        {
            Sut = new FluentFTPDownloaderClient();

            await base.DeleteResourceAsyncOperationCanceledException();
        }

        #endregion

        #region Disconnect

        public override async Task DisconnectAsyncSucceeded()
        {
            var client = new Mock<IAsyncFtpClient>();

            client.SetupGet(p => p.IsConnected).Returns(true);

            client.Setup(s => s.Disconnect(default)).Callback(() => client.SetupGet(p => p.IsConnected).Returns(false));

            Sut = new FluentFTPDownloaderClient(client.Object);

            await base.DisconnectAsyncSucceeded();
        }

        public override async Task DisconnectAsyncNotConnected()
        {
            var client = new Mock<IAsyncFtpClient>();

            Sut = new FluentFTPDownloaderClient(client.Object);

            await base.DisconnectAsyncNotConnected();
        }

        public override async Task DisconnectAsyncControllerConnectionException()
        {
            var client = new Mock<IAsyncFtpClient>();

            client.Setup(p => p.IsConnected).Returns(true);

            client.Setup(s => s.Disconnect(default)).Throws<Exception>();

            Sut = new FluentFTPDownloaderClient(client.Object);

            await base.DisconnectAsyncControllerConnectionException();
        }

        public override async Task DisconnectAsyncOperationCanceledException()
        {
            Sut = new FluentFTPDownloaderClient();

            base.DisconnectAsyncOperationCanceledException();
        }

        #endregion

        #region DownloadResource

        public override async Task DownloadResourceAsyncSucceed()
        {
            var client = new Mock<IAsyncFtpClient>();

            client.SetupGet(p => p.IsConnected).Returns(true);

            client.Setup(s => s.DownloadFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<FtpLocalExists>(), It.IsAny<FtpVerify>(), It.IsAny<IProgress<FtpProgress>>(), default))
                .ReturnsAsync((string a, string b, FtpLocalExists c, FtpVerify d, IProgress<FtpProgress> e, CancellationToken f) => FtpStatus.Success);

            Sut = new FluentFTPDownloaderClient(client.Object);

            await base.DownloadResourceAsyncSucceed();
        }

        public override async Task DownloadResourceAsyncNullLocal()
        {
            var client = new Mock<IAsyncFtpClient>();

            client.SetupGet(p => p.IsConnected).Returns(true);

            Sut = new FluentFTPDownloaderClient(client.Object);

            await base.DownloadResourceAsyncNullLocal();
        }

        public override async Task DownloadResourceAsyncNullRemote()
        {
            var client = new Mock<IAsyncFtpClient>();

            client.SetupGet(p => p.IsConnected).Returns(true);

            Sut = new FluentFTPDownloaderClient(client.Object);

            await base.DownloadResourceAsyncNullRemote();
        }

        public override async Task DownloadResourceAsyncInvalidLocal()
        {
            var client = new Mock<IAsyncFtpClient>();

            client.SetupGet(p => p.IsConnected).Returns(true);

            Sut = new FluentFTPDownloaderClient(client.Object);

            await base.DownloadResourceAsyncInvalidLocal();
        }

        public override async Task DownloadResourceAsyncNotConnected()
        {
            var client = new Mock<IAsyncFtpClient>();

            Sut = new FluentFTPDownloaderClient(client.Object);

            await base.DownloadResourceAsyncNotConnected();
        }

        public override async Task DownloadResourceAsyncDownloaderClientDownloadResourceException()
        {
            var client = new Mock<IAsyncFtpClient>();

            client.Setup(p => p.IsConnected).Returns(true);

            client.Setup(s => s.DownloadFile(It.IsAny<string>(), "", FtpLocalExists.Overwrite, FtpVerify.None, null, default)).Throws<Exception>();

            Sut = new FluentFTPDownloaderClient(client.Object);

            await base.DownloadResourceAsyncDownloaderClientDownloadResourceException();
        }

        public override async Task DownloadResourceAsyncOperationCanceledException()
        {
            Sut = new FluentFTPDownloaderClient();

            await base.DownloadResourceAsyncOperationCanceledException();
        }

        #endregion

        #region ListResources

        public override async Task ListResourcesAsyncSucceeded()
        {
            var client = new Mock<IAsyncFtpClient>();
            var ftpListItem = new Mock<FtpListItem>();

            client.SetupGet(p => p.Host).Returns("localhost");
            client.SetupGet(p => p.Port).Returns(21);
            client.SetupGet(p => p.IsConnected).Returns(true);

            client.Setup(s => s.GetListing(It.IsAny<string>(), It.IsAny<FtpListOption>(), default))
               .ReturnsAsync((string a, FtpListOption b, CancellationToken c) =>
               [
                   new FtpListItem("test", 0, FtpObjectType.File, DateTime.Now) { FullName = a }
               ]);

            Sut = new FluentFTPDownloaderClient(client.Object);

            await base.ListResourcesAsyncSucceeded();
        }

        public override async Task ListResourcesAsyncNotConnected()
        {
            var client = new Mock<IAsyncFtpClient>();

            Sut = new FluentFTPDownloaderClient(client.Object);

            await base.ListResourcesAsyncNotConnected();
        }

        public override async Task ListDirectoryAsyncControllerDownloadFileException()
        {
            var client = new Mock<IAsyncFtpClient>();

            client.Setup(p => p.IsConnected).Returns(true);

            client.Setup(s => s.GetListing(It.IsAny<string>(), FtpListOption.Auto, default)).Throws<Exception>();

            Sut = new FluentFTPDownloaderClient(client.Object);

            await base.ListDirectoryAsyncControllerDownloadFileException();
        }

        public override async Task ListResourcesAsyncDownloaderClientListResourcesException()
        {
            Sut = new FluentFTPDownloaderClient();

            await base.ListResourcesAsyncDownloaderClientListResourcesException();
        }

        #endregion

        public override async Task ConnectAsyncConnectionProperties()
        {
            var client = new Mock<IAsyncFtpClient>();
            var config = new Mock<FtpConfig>();

            client.SetupAllProperties();

            client.Setup(s => s.AutoConnect(default)).Callback(() => client.SetupGet(p => p.IsConnected).Returns(true));

            Client = client.Object;
            Client.Config = config.Object;
            Sut = new FluentFTPDownloaderClient(Client);

            await base.ConnectAsyncConnectionProperties();
        }

        public override bool VerifyIpAddress(IAsyncFtpClient client, string ipAddress)
        {
            return client.Host == ipAddress.ToString();
        }

        public override bool VerifyPort(IAsyncFtpClient client, int port)
        {
            return client.Port == port;
        }

        public override bool VerifyUserName(IAsyncFtpClient client, string userName)
        {
            return client.Credentials.UserName == userName;
        }

        public override bool VerifyPassword(IAsyncFtpClient client, string password)
        {
            return client.Credentials.Password == password;
        }

        public override bool VerifyConnectionTimeout(IAsyncFtpClient client, int connectionTimeout)
        {
            return client.Config.ConnectTimeout == connectionTimeout;
        }

        public override bool VerifyOperationTimeout(IAsyncFtpClient client, int operationTimeout)
        {
            return client.Config.ReadTimeout == operationTimeout;
        }
    }
}
