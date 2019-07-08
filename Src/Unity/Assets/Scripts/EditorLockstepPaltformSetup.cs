using System;
using System.Collections;
using System.IO;
using Lockstep.Game;
using Lockstep.Util;
using UnityEditor;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;



using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Text;
using Lockstep.Game;
using Lockstep.Util;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using ICSharpCode.SharpZipLib.Zip;


public class ZipUtil{
    static int BufferSize = 2048;

    /// <summary>
    /// 解压文件到指定文件夹
    /// </summary>
    /// <param name="sourceFile">压缩文件</param>
    /// <param name="destinationDirectory">目标文件夹，如果为空则解压到当前文件夹下</param>
    /// <param name="password">密码</param>
    /// <returns></returns>
    public static bool Decompress(string sourceFile, string destinationDirectory = null, string password = null){
        bool result = false;

        if (!File.Exists(sourceFile)) {
            throw new FileNotFoundException("要解压的文件不存在", sourceFile);
        }

        if (string.IsNullOrWhiteSpace(destinationDirectory)) {
            destinationDirectory = Path.GetDirectoryName(sourceFile);
        }

        try {
            if (!Directory.Exists(destinationDirectory)) {
                Directory.CreateDirectory(destinationDirectory);
            }

            var stream = File.Open(sourceFile, FileMode.Open, FileAccess.Read, FileShare.Read);
            using (ZipInputStream zipStream = new ZipInputStream(stream)) {
                zipStream.Password = password;
                ZipEntry zipEntry = zipStream.GetNextEntry();

                while (zipEntry != null) {
                    if (zipEntry.IsDirectory) //如果是文件夹则创建
                    {
                        Directory.CreateDirectory(Path.Combine(destinationDirectory,
                            Path.GetDirectoryName(zipEntry.Name)));
                    }
                    else {
                        string fileName = Path.GetFileName(zipEntry.Name);
                        if (!string.IsNullOrEmpty(fileName) && fileName.Trim().Length > 0) {
                            FileInfo fileItem = new FileInfo(Path.Combine(destinationDirectory, zipEntry.Name));
                            using (FileStream writeStream = fileItem.Create()) {
                                byte[] buffer = new byte[BufferSize];
                                int readLength = 0;

                                do {
                                    readLength = zipStream.Read(buffer, 0, BufferSize);
                                    writeStream.Write(buffer, 0, readLength);
                                } while (readLength == BufferSize);

                                writeStream.Flush();
                                writeStream.Close();
                            }

                            fileItem.LastWriteTime = zipEntry.DateTime;
                        }
                    }

                    zipEntry = zipStream.GetNextEntry(); //获取下一个文件
                }

                zipStream.Close();
            }

            result = true;
        }
        catch (System.Exception ex) {
            throw new Exception("文件解压发生错误", ex);
        }

        return result;
    }
}


public class HttpTask {
    public string url;
    public long downloadSize;
    public long totalSize;
    public float progress;
    public FileStream stream;
}

public class HttpUtil {
    private const int BufferSize = 2048;
    public static Thread downloadThread;

    
    public static void DoInit(){
        downloadThread = new Thread(ThreadUpdate);
        smp = new Semaphore(0, 1);
        downloadThread.Start();
    }

    public static void Stop(){
        if (downloadThread != null) {
            downloadThread.Abort();
        }

        downloadThread = null;
    }

    private static Semaphore smp;
    static Queue<HttpTask> tasks = new Queue<HttpTask>();

    public static void AddTask(HttpTask task){
        if (downloadThread == null) {
            Debug.LogError(" HttpUtil do not has init!");
            task.progress = 1;
            return;
        }

        lock (tasks) {
            tasks.Enqueue(task);
            smp.Release(1);
        }
    }

    static void ThreadUpdate(){
        while (true) {
            HttpTask task = null;
            lock (tasks) {
                if (tasks.Count > 0) {
                    task = tasks.Dequeue();
                }
            }

            if (task == null) {
                smp.WaitOne();
                continue;
            }
            DownLoadFile(task);
        }
    }

    static byte[] _tempBuffer = new byte[BufferSize];
    public static void DownLoadFile(HttpTask task){
        var url = task.url;
        task.progress = 0;
        FileStream outputStream = task.stream;
        WebRequest request = WebRequest.Create(url);
        HttpWebResponse response = (HttpWebResponse) request.GetResponse();
        Stream httpStream = response.GetResponseStream();
        task.totalSize = response.ContentLength;
        int readCount = httpStream.Read(_tempBuffer, 0, BufferSize);
        task.downloadSize = 0;
        var initTimer = DateTime.Now;
        while (readCount > 0) {
            outputStream.Write(_tempBuffer, 0, readCount);
            readCount = httpStream.Read(_tempBuffer, 0, BufferSize);
            task.downloadSize += readCount;
            task.progress = (1.0f * task.downloadSize) / task.totalSize;
        }
        task.progress = 1;
        httpStream.Close();
        outputStream.Close();
        response.Close();
    }
}
