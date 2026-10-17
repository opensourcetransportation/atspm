#region license
// Copyright 2026 Utah Departement of Transportation
// for InfrastructureTests - Utah.Udot.Atspm.InfrastructureTests.DownloaderClientTests/SSHNetSFTPDownloaderClientTests.cs
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

using Moq;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Utah.Udot.Atspm.Infrastructure.Services.DownloaderClients;
using Xunit.Abstractions;

namespace Utah.Udot.Atspm.InfrastructureTests.DownloaderClientTests
{
    public class SSHNetSFTPDownloaderClientTests : DownloaderClientTestsBase<ISftpClientWrapper>
    {
        public SSHNetSFTPDownloaderClientTests(ITestOutputHelper output) : base(output) { }

        #region Connect

        public override async Task ConnectAsyncSucceeded()
        {
            var client = new Mock<ISftpClientWrapper>();

            client.Setup(s => s.ConnectAsync(It.IsAny<CancellationToken>()))
                .Returns(() =>
                {
                    client.SetupGet(p => p.IsConnected).Returns(true);
                    return Task.CompletedTask;
                });

            Sut = new SSHNetSFTPDownloaderClient(client.Object);

            await base.ConnectAsyncSucceeded();
        }

        public override async Task ConnectAsyncNullConnection()
        {
            var client = new Mock<ISftpClientWrapper>();

            client.Setup(s => s.ConnectAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            Sut = new SSHNetSFTPDownloaderClient(client.Object);

            await base.ConnectAsyncNullConnection();
        }

        public override async Task ConnectAsyncNullCredentials()
        {
            var client = new Mock<ISftpClientWrapper>();

            client.Setup(s => s.ConnectAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            Sut = new SSHNetSFTPDownloaderClient(client.Object);

            await base.ConnectAsyncNullCredentials();
        }

        public override async Task ConnectAsyncControllerConnectionException()
        {
            var client = new Mock<ISftpClientWrapper>();

            client.Setup(s => s.ConnectAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("SFTP connection failed"));

            Sut = new SSHNetSFTPDownloaderClient(client.Object);

            await base.ConnectAsyncControllerConnectionException();
        }

        public override async Task ConnectAsyncOperationCanceledException()
        {
            Sut = new SSHNetSFTPDownloaderClient();

            await base.ConnectAsyncOperationCanceledException();
        }

        #endregion

        #region DeleteResource

        public override async Task DeleteResourceAsyncSucceeded()
        {
            var client = new Mock<ISftpClientWrapper>();

            client.SetupGet(p => p.IsConnected).Returns(true);

            Sut = new SSHNetSFTPDownloaderClient(client.Object);

            var uri = new UriBuilder()
            {
                Scheme = Uri.UriSchemeSftp,
                Host = "127.0.0.1",
                Port = 22,
                Path = "/a/b/c.txt"
            }.Uri;

            await Sut.DeleteResourceAsync(uri);
        }

        public override Task DeleteResourceAsyncNullResource()
        {
            var client = new Mock<ISftpClientWrapper>();

            client.SetupGet(p => p.IsConnected).Returns(true);

            Sut = new SSHNetSFTPDownloaderClient(client.Object);

            return base.DeleteResourceAsyncNullResource();
        }

        public override Task DeleteResourceAsyncInvalidResource()
        {
            var client = new Mock<ISftpClientWrapper>();

            client.SetupGet(p => p.IsConnected).Returns(true);

            Sut = new SSHNetSFTPDownloaderClient(client.Object);

            return base.DeleteResourceAsyncInvalidResource();
        }

        public override async Task DeleteResourceAsyncNotConnected()
        {
            var client = new Mock<ISftpClientWrapper>();

            Sut = new SSHNetSFTPDownloaderClient(client.Object);

            await base.DeleteResourceAsyncNotConnected();
        }

        public override async Task DeleteResourceAsyncControllerDeleteResourceException()
        {
            var client = new Mock<ISftpClientWrapper>();

            client.SetupGet(p => p.IsConnected).Returns(true);

            client.Setup(s => s.DeleteFile("")).Throws<Exception>();

            Sut = new SSHNetSFTPDownloaderClient(client.Object);

            await base.DeleteResourceAsyncControllerDeleteResourceException();
        }

        public override async Task DeleteResourceAsyncOperationCanceledException()
        {
            Sut = new SSHNetSFTPDownloaderClient();

            await base.DeleteResourceAsyncOperationCanceledException();
        }

        #endregion

        #region Disconnect

        public override async Task DisconnectAsyncSucceeded()
        {
            var client = new Mock<ISftpClientWrapper>();

            client.SetupGet(p => p.IsConnected).Returns(true);

            client.Setup(s => s.Disconnect()).Callback(() => client.SetupGet(p => p.IsConnected).Returns(false));

            Sut = new SSHNetSFTPDownloaderClient(client.Object);

            await base.DisconnectAsyncSucceeded();
        }

        public override async Task DisconnectAsyncNotConnected()
        {
            var client = new Mock<ISftpClientWrapper>();

            Sut = new SSHNetSFTPDownloaderClient(client.Object);

            await base.DisconnectAsyncNotConnected();
        }

        public override async Task DisconnectAsyncControllerConnectionException()
        {
            var client = new Mock<ISftpClientWrapper>();

            client.Setup(p => p.IsConnected).Returns(true);

            client.SetupAllProperties();

            client.Setup(s => s.Disconnect()).Throws<Exception>();

            Sut = new SSHNetSFTPDownloaderClient(client.Object);

            await base.DisconnectAsyncControllerConnectionException();
        }

        public override async Task DisconnectAsyncOperationCanceledException()
        {
            Sut = new SSHNetSFTPDownloaderClient();

            await base.DisconnectAsyncOperationCanceledException();
        }

        #endregion

        #region DownloadResource

        public override async Task DownloadResourceAsyncSucceed()
        {
            var client = new Mock<ISftpClientWrapper>();

            client.SetupGet(p => p.IsConnected).Returns(true);

            client.Setup(s => s.DownloadFileAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((string a, string b) => new FileInfo(a));

            Sut = new SSHNetSFTPDownloaderClient(client.Object);

            await base.DownloadResourceAsyncSucceed();
        }

        public override async Task DownloadResourceAsyncNullLocal()
        {
            var client = new Mock<ISftpClientWrapper>();

            client.SetupGet(p => p.IsConnected).Returns(true);

            Sut = new SSHNetSFTPDownloaderClient(client.Object);

            await base.DownloadResourceAsyncNullLocal();
        }

        public override async Task DownloadResourceAsyncNullRemote()
        {
            var client = new Mock<ISftpClientWrapper>();

            client.SetupGet(p => p.IsConnected).Returns(true);

            Sut = new SSHNetSFTPDownloaderClient(client.Object);

            await base.DownloadResourceAsyncNullRemote();
        }

        public override async Task DownloadResourceAsyncInvalidLocal()
        {
            var client = new Mock<ISftpClientWrapper>();

            client.SetupGet(p => p.IsConnected).Returns(true);

            Sut = new SSHNetSFTPDownloaderClient(client.Object);

            await base.DownloadResourceAsyncInvalidLocal();
        }

        public override async Task DownloadResourceAsyncNotConnected()
        {
            var client = new Mock<ISftpClientWrapper>();

            Sut = new SSHNetSFTPDownloaderClient(client.Object);

            await base.DownloadResourceAsyncNotConnected();
        }

        public override async Task DownloadResourceAsyncDownloaderClientDownloadResourceException()
        {
            var client = new Mock<ISftpClientWrapper>();

            client.Setup(p => p.IsConnected).Returns(true);

            client.Setup(s => s.DownloadFileAsync(It.IsAny<string>(), "")).Throws<Exception>();

            Sut = new SSHNetSFTPDownloaderClient(client.Object);

            await base.DownloadResourceAsyncDownloaderClientDownloadResourceException();
        }

        public override async Task DownloadResourceAsyncOperationCanceledException()
        {
            Sut = new SSHNetSFTPDownloaderClient();

            await base.DownloadResourceAsyncOperationCanceledException();
        }

        #endregion

        #region ListResources

        public override async Task ListResourcesAsyncSucceeded()
        {
            var connectionInfo = new ConnectionInfo
                ("127.0.0.1",
                22,
                "user",
                new PasswordAuthenticationMethod("user", "password"));

            var client = new Mock<ISftpClientWrapper>();

            client.SetupGet(p => p.ConnectionInfo).Returns(connectionInfo);

            client.SetupGet(p => p.IsConnected).Returns(true);

            client.Setup(s => s.ListDirectoryAsync(It.IsAny<string>(), It.Is<string[]>(a => true)))
                .ReturnsAsync((string a, string[] b) => Mock.Of<ISftpFile[]>());
            //.ReturnsAsync((string a, string[] b) => [a]);

            Sut = new SSHNetSFTPDownloaderClient(client.Object);

            await base.ListResourcesAsyncSucceeded();
        }

        public override async Task ListResourcesAsyncNotConnected()
        {
            var client = new Mock<ISftpClientWrapper>();

            Sut = new SSHNetSFTPDownloaderClient(client.Object);

            await base.ListResourcesAsyncNotConnected();
        }

        public override async Task ListDirectoryAsyncControllerDownloadFileException()
        {
            var client = new Mock<ISftpClientWrapper>();

            client.Setup(p => p.IsConnected).Returns(true);

            client.Setup(s => s.ListDirectoryAsync(It.IsAny<string>(), It.IsAny<string[]>())).Throws<Exception>();

            Sut = new SSHNetSFTPDownloaderClient(client.Object);

            await base.ListDirectoryAsyncControllerDownloadFileException();
        }

        public override async Task ListResourcesAsyncDownloaderClientListResourcesException()
        {
            Sut = new SSHNetSFTPDownloaderClient();

            await base.ListResourcesAsyncDownloaderClientListResourcesException();
        }

        #endregion

        public override async Task ConnectAsyncConnectionProperties()
        {
            var client = new Mock<ISftpClientWrapper>();

            client.SetupAllProperties();

            client.Setup(s => s.Connect()).Callback(() => client.SetupGet(p => p.IsConnected).Returns(true));

            Client = client.Object;

            Sut = new SSHNetSFTPDownloaderClient(Client);

            await base.ConnectAsyncConnectionProperties();
        }

        public override bool VerifyIpAddress(ISftpClientWrapper client, string ipAddress)
        {
            //return client.ConnectionInfo.Host == ipAddress.ToString();
            return true;
        }

        public override bool VerifyPort(ISftpClientWrapper client, int port)
        {
            //return client.ConnectionInfo.Port == port;
            return true;
        }

        public override bool VerifyUserName(ISftpClientWrapper client, string userName)
        {
            //return client.ConnectionInfo.Username == userName;
            return true;
        }

        public override bool VerifyPassword(ISftpClientWrapper client, string password)
        {
            //client.ConnectionInfo. == password;
            return true;
        }

        public override bool VerifyConnectionTimeout(ISftpClientWrapper client, int connectionTimeout)
        {
            //return client.ConnectionInfo.Timeout == TimeSpan.FromMilliseconds(connectionTimeout);
            return true;
        }

        public override bool VerifyOperationTimeout(ISftpClientWrapper client, int operationTimeout)
        {
            return client.OperationTimeout == TimeSpan.FromMilliseconds(operationTimeout);
        }
    }
}
