# CryptoUtilsConfig

```CryptoUtilsConfig``` is used to configure the ```ICryptoUtils``` implementation used to verify the signature on the incoming requests as per the [HTTP signature verification spec](https://smartthings.developer.samsung.com/docs/smartapps/webhook-apps.html#HTTP-signature-verification).  The ```CryptoUtilsConfig``` should be configured with the path to a file containing the public key content you get from your sapp regitration.

## Example Config

```
  "CryptoUtilsConfig": {
    "PublicKeyFilePath": "Keys/GWPubKey.pem"
  },
```

## Example Key File Contents

```
-----BEGIN PUBLIC KEY-----
sdfssdfsfsdfsdfsdfsdf+M51NU7QaV
542Yca7zBQ41BGLkGHPkqkmLC/+dfgdfgdfgdfgdfgdfgdfgdfgdfg
dfgdfgdfg/dgdfg/sdfsdf/e0BPxgc9mNgGAKsRjpdEM5qvSikwIlIjdgIdK
eMXiRpPR85r8ofrjZKzHU7ncNbcbunEFLoLaGDrKszGLQBS8xD5gGsQWsF0vhn3ErhvYC5KQfuxC
ddgfdfgdfgdfgdfgdfgdfgdfgdfgdfgdfgdfg
-----END PUBLIC KEY-----
```
