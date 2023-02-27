// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

namespace DataImport.Web
{
    public class DataProtectionOptions
    {
        //public string CertificateFilePath { get; set; }
        //public string CertificatePassword { get; set; }
        public string PemCertFilePath { get; set; }
        public string PemKeyFilePath { get; set; }
        public string KeyConnectionString { get; set; }
        public string KeyName { get; set; }
        public bool IsClusterEnvironment { get; set; }
    }
}
