﻿using Org.BouncyCastle.Bcpg.OpenPgp;
using System;
using System.IO;
using System.Linq;

namespace PGP.Tools.Standard
{
    internal sealed class PGPEncryptionKeys
    {
        #region Instance Members (Public)

        public PgpPublicKey PublicKey { get; private set; }
        public PgpPrivateKey PrivateKey { get; private set; }
        public PgpSecretKey SecretKey { get; private set; }

        #endregion Instance Members (Public)

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the EncryptionKeys class.
        /// Two keys are required to encrypt and sign data. Your private key and the recipients public key.
        /// The data is encrypted with the recipients public key and signed with your private key.
        /// </summary>
        /// <param name="publicKeyFilePath">The key used to encrypt the data</param>
        /// <param name="privateKeyFilePath">The key used to sign the data.</param>
        /// <param name="passPhrase">The password required to access the private key</param>
        /// <exception cref="ArgumentException">Public key not found. Private key not found. Missing password</exception>
        public PGPEncryptionKeys(string publicKeyFilePath, string privateKeyFilePath, string passPhrase)
        {
            if (String.IsNullOrEmpty(publicKeyFilePath))
                throw new ArgumentException("PublicKeyFilePath");
            if (String.IsNullOrEmpty(privateKeyFilePath))
                throw new ArgumentException("PrivateKeyFilePath");
            if (passPhrase == null)
                throw new ArgumentNullException("Invalid Pass Phrase.");

            if (!File.Exists(publicKeyFilePath))
                throw new FileNotFoundException(String.Format("Public Key file [{0}] does not exist.", publicKeyFilePath));
            if (!File.Exists(privateKeyFilePath))
                throw new FileNotFoundException(String.Format("Private Key file [{0}] does not exist.", privateKeyFilePath));

            PublicKey = ReadPublicKey(publicKeyFilePath);
            SecretKey = ReadSecretKey(privateKeyFilePath);
            PrivateKey = ReadPrivateKey(passPhrase);
        }

        #endregion Constructors

        #region Secret Key

        private PgpSecretKey ReadSecretKey(string privateKeyPath)
        {
            using (Stream sr = File.OpenRead(privateKeyPath))
            {
                using (Stream inputStream = PgpUtilities.GetDecoderStream(sr))
                {
                    PgpSecretKeyRingBundle secretKeyRingBundle = new PgpSecretKeyRingBundle(inputStream);
                    PgpSecretKey foundKey = GetFirstSecretKey(secretKeyRingBundle);
                    if (foundKey != null)
                        return foundKey;
                }
            }
            throw new ArgumentException("Can't find signing key in key ring.");
        }

        /// <summary>
        /// Return the first key we can use to encrypt.
        /// Note: A file can contain multiple keys (stored in "key rings")
        /// </summary>
        private PgpSecretKey GetFirstSecretKey(PgpSecretKeyRingBundle secretKeyRingBundle)
        {
            foreach (PgpSecretKeyRing kRing in secretKeyRingBundle.GetKeyRings())
            {
                PgpSecretKey key = kRing.GetSecretKeys()
                    .Cast<PgpSecretKey>()
                    .Where(k => k.IsSigningKey)
                    .FirstOrDefault();
                if (key != null)
                    return key;
            }
            return null;
        }

        #endregion Secret Key

        #region Public Key

        private PgpPublicKey ReadPublicKey(string publicKeyPath)
        {
            using (Stream keyIn = File.OpenRead(publicKeyPath))
            {
                using (Stream inputStream = PgpUtilities.GetDecoderStream(keyIn))
                {
                    PgpPublicKeyRingBundle publicKeyRingBundle = new PgpPublicKeyRingBundle(inputStream);
                    PgpPublicKey foundKey = GetFirstPublicKey(publicKeyRingBundle);
                    if (foundKey != null)
                        return foundKey;
                }
            }
            throw new ArgumentException("No encryption key found in public key ring.");
        }

        private PgpPublicKey GetFirstPublicKey(PgpPublicKeyRingBundle publicKeyRingBundle)
        {
            foreach (PgpPublicKeyRing kRing in publicKeyRingBundle.GetKeyRings())
            {
                PgpPublicKey key = kRing.GetPublicKeys()
                    .Cast<PgpPublicKey>()
                    .Where(k => k.IsEncryptionKey)
                    .FirstOrDefault();
                if (key != null)
                    return key;
            }
            return null;
        }

        #endregion Public Key

        #region Private Key

        private PgpPrivateKey ReadPrivateKey(string passPhrase)
        {
            PgpPrivateKey privateKey = SecretKey.ExtractPrivateKey(passPhrase.ToCharArray());
            if (privateKey != null)
                return privateKey;

            throw new ArgumentException("No private key found in secret key.");
        }

        #endregion Private Key
    }

}