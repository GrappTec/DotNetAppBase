﻿using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Grapp.AppBase.Std.Exceptions.Assert;

namespace Grapp.AppBase.Std.Library
{
    public partial class XHelper
    {
        public static class Security
        {
            public static class Certificate
            {
                public static X509Certificate2 RecoverCertificateBySerialNumber(
                    string serialNumber, StoreName storeName = StoreName.My, StoreLocation storeLocation = StoreLocation.LocalMachine)
                {
                    XContract.ArgIsNotNull(serialNumber, nameof(serialNumber));

                    using(var store = new X509Store(storeName, storeLocation))
                    {
                        store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);

                        return store
                            .Certificates
                            .Cast<X509Certificate2>()
                            .FirstOrDefault(certificate => serialNumber.Equals(certificate.SerialNumber, StringComparison.InvariantCultureIgnoreCase));
                    }
                }

#if !NETSTANDARD
                public static IEnumerable<X509Certificate2> ShowSelectionUIFromValidCertificates(X509SelectionFlag selectionModel = X509SelectionFlag.SingleSelection)
                {
                    var uiMessage = selectionModel == X509SelectionFlag.SingleSelection
                                        ? DbMessages.Certificate_ShowSelectionUIFromValidCertificates_Selecione_o_certificado_que_deseja_utilizar
                                        : DbMessages.Certificate_ShowSelectionUIFromValidCertificates_Selecione_os_certificados_que_deseja_utilizar;

                    var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                    store.Open(flags: OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);

                    try
                    {
                        var certificates = store.Certificates.Find(X509FindType.FindByTimeValid, DateTime.Now, validOnly: true);
                        var selection = X509Certificate2UI.SelectFromCollection(
                            certificates, DbMessages.Certificate_ShowSelectionUIFromValidCertificates_Certificados_válidos_, uiMessage, selectionModel);

                        return selection.Cast<X509Certificate2>().ToArray();
                    }
                    finally
                    {
                        store.Close();
                    }
                }
#endif
            }
        }
    }
}