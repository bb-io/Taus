﻿using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Taus.Base;
public class FileManager : IFileManagementClient
{
    private readonly string _inputFolder;
    private readonly string _outputFolder;

    public FileManager()
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var projectDirectory = Directory.GetParent(baseDirectory)?.Parent?.Parent?.Parent?.FullName
                               ?? throw new Exception("Couldn't find project directory");

        var testFilesPath = Path.Combine(projectDirectory, "TestFiles");
        _inputFolder = Path.Combine(testFilesPath, "Input");
        _outputFolder = Path.Combine(testFilesPath, "Output");

        Directory.CreateDirectory(_inputFolder);
        Directory.CreateDirectory(_outputFolder);
    }

    public Task<Stream> DownloadAsync(FileReference reference)
    {
        var path = Path.Combine(_inputFolder, reference.Name);
        Assert.IsTrue(File.Exists(path), $"File not found at: {path}");
        var bytes = File.ReadAllBytes(path);

        var stream = new MemoryStream(bytes);
        return Task.FromResult((Stream)stream);
    }

    public Task<FileReference> UploadAsync(Stream stream, string contentType, string fileName)
    {
        var path = Path.Combine(_outputFolder, fileName);
        new FileInfo(path).Directory?.Create();
        using (var fileStream = File.Create(path))
        {
            stream.CopyTo(fileStream);
        }

        return Task.FromResult(new FileReference { Name = fileName, ContentType = contentType });
    }
}
