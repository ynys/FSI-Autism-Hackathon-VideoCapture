﻿using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Autism_Video_API.Models
{
    public class PathfinderVideo
    {
        public string PatientID { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string FileName { get; set; }
    
        public PathfinderVideo(string patientID, string startTime, string endTime, string fileName) 
        {
            this.PatientID = patientID;
            this.StartTime = startTime;
            this.EndTime = endTime;
            this.FileName = fileName;
        }

        public string Save(string StorageConnectionString)
        {
            var ve = new VideoEntity(PatientID, StartTime, EndTime, FileName, StorageConnectionString);

            //Parse the connection string and return a reference to the storage account.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["BlobConnectionString"]);

            //Create the blob client object.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            //Get a reference to a container to use for the sample code, and create it if it does not exist.
            CloudBlobContainer container = blobClient.GetContainerReference("testpublic");
            container.CreateIfNotExists();



            //Get a reference to a blob within the container.
            CloudBlockBlob blob = container.GetBlockBlobReference(FileName);

            //Set the expiry time and permissions for the blob.
            //In this case, the start time is specified as a few minutes in the past, to mitigate clock skew.
            //The shared access signature will be valid immediately.
            SharedAccessBlobPolicy sasConstraints = new SharedAccessBlobPolicy();
            sasConstraints.SharedAccessStartTime = DateTimeOffset.UtcNow.AddMinutes(-5);
            sasConstraints.SharedAccessExpiryTime = DateTimeOffset.UtcNow.AddHours(2);
            sasConstraints.Permissions = SharedAccessBlobPermissions.Write;

            //Generate the shared access signature on the blob, setting the constraints directly on the signature.
            string sasBlobToken = blob.GetSharedAccessSignature(sasConstraints);

            //Return the URI string for the container, including the SAS token.
            return blob.Uri + sasBlobToken;

        }
    }
}
