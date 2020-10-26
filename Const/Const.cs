using System;

namespace Const
{
    public static class Const
    {
        public const string Ip = "127.0.0.1";
        public const string Title = "Ieskite pranesyma:Check - 0;UseCode - 1; ActionGenerate - 2;Stop Server - 3";
        public const string Check = "Check";
        public const string UseCode = "UseCode";
        public const string ActionCodeInput = "Iveskite Akcijos Koda";
        public const string CardNumberInput = "Iveskite korteles numeri";
        public const string StopServer = "ServerStop";
        public const string DataSent = "Duomenys issiusti";
        public const string Error = "Error";
        public const string Statement = "Pranesymas: ";
        public const string GenerateAction = "GenerateAction";
        public const string WritedCodes = "Kodai Irasyti";
        public const int port = 8000;
        public const int maxActionCodeLength = 8;
        public const string AkctionPath = @"C:\Users\vgds7\source\repos\ClientServerSocket\Akcijos.txt";
        public const string KoduSimboliai = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
        public const string AkcijosKoduSimboliai = "1234567890";
        public const string CardNumberPath = @"C:\Users\vgds7\source\repos\ClientServerSocket\CardNumber.txt";
        public const string UsedActionCodes = @"C:\Users\vgds7\source\repos\ClientServerSocket\PanaudotiAkcijosKodai.txt";
    }
}
