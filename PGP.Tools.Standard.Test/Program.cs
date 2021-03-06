﻿using System;
using System.IO;

namespace PGP.Tools.Standard.Test
{
    class Program
    {
        const string identityString = "test@email.com";
        const string passwordString = "password123";
        static void Main(string[] args)
        {
            Console.WriteLine(Directory.GetCurrentDirectory());
            //GenerateKeyPair("test");
            EncryptFile();
            DecryptFile();
            //EncryptFileWithString();
            //DecryptFileWithString();
            //EncryptFileNSign();
            //DecryptFileNVerify();
            Console.ReadLine();
        }

        private static void GenerateKeyPair(string keyName = null)
        {
            keyName = keyName ?? "Sample";
            using (PGP.KeyGenerator pgp = new PGP.KeyGenerator())
                pgp.GenerateKeyPath(
                    publicKeyFilePath: $"{keyName}_public_key.asc",
                    privateKeyFilePath: $"{keyName}_private_key.asc",
                    identity: $"{identityString} <{keyName}>",
                    password: passwordString);

            Console.WriteLine("PGP KeyPair generated.");
        }
        private static void EncryptFile()
        {
            using (PGP.Encrypt pgp = new PGP.Encrypt())
            {
                pgp.FileType = Enums.PGPFileType.UTF8;

                pgp.EncryptFileWithPathKey(
                    inputFilePath: "Sample_file.txt",
                    outputFilePath: "Sample_file.txt.pgp",
                    publicKeyFilePath: "Sample_public_key.asc",
                    armor: true,
                    withIntegrityCheck: false);
                Console.WriteLine("PGP Encryption done.");
            }
        }
        private static void DecryptFile()
        {
            using (PGP.Decrypt pgp = new PGP.Decrypt())
            {
                pgp.DecryptFileWithPath(
                    inputFilePath: "Sample_file.txt.pgp",
                    outputFilePath: "Sample_file.out.txt",
                    privateKeyFilePath: "Sample_private_key.asc",
                    passPhrase: "password123");
                Console.WriteLine("PGP Decryption done.");
            }
        }
        private static void EncryptFileWithString()
        {
            using (PGP.Encrypt pgp = new PGP.Encrypt())
            {
                pgp.EncryptFileWithStringKey(
                    inputFilePath: "Sample_file.txt",
                    outputFilePath: "Sample_file_stream.txt.pgp",
                    publicKeyString: Constants.publicKey,
                    armor: true,
                    withIntegrityCheck: false);
                Console.WriteLine("PGP Steam public key Encryption done.");
            }
        }
        private static void DecryptFileWithString()
        {
            using (PGP.Decrypt pgp = new PGP.Decrypt())
            {
                pgp.DecryptFileWithStringKey(
                inputFilePath: "Sample_file_stream.txt.pgp",
                outputFilePath: "Sample_file_stream.out.txt",
                privateKeyString: Constants.privateKey,
                passPhrase: "password123");
                Console.WriteLine("PGP Stream private key Decryption done.");
            }
        }
        private static void EncryptFileNSign()
        {
            using (PGP.Encrypt pgp = new PGP.Encrypt())
            {
                pgp.EncryptFileAndSign(
                    inputFilePath: "Sample_file.txt",
                    outputFilePath: "Sample_file.nisgn.txt.pgp",
                    publicKeyFilePath: "Sample_public_key.asc",
                    privateKeyFilePath: "Sample_private_key.asc",
                    passPhrase: "password123",
                    armor: true,
                    withIntegrityCheck: false);
                Console.WriteLine("PGP Encryption done.");
            }
        }
        private static void DecryptFileNVerify()
        {
            using (PGP.Decrypt pgp = new PGP.Decrypt())
            {
                pgp.DecryptFileAndVerify(
                   inputFilePath: "Sample_file.nisgn.txt.pgp",
                   outputFilePath: "Sample_file.nisgn.out.txt",
                   publicKeyFilePath: "Sample_public_key.asc",
                   privateKeyFilePath: "Sample_private_key.asc",
                   passPhrase: "password123");
                Console.WriteLine("PGP Decryption done.");
            }
        }
    }
}
