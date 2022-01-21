# How to make Token authorized AES encrypted HLS stream working in Safari

This code sample is a web proxy for making Token Authroized AES encrypted HLS stream working in Safari. Please check out this blog for more details: https://azure.microsoft.com/en-us/blog/how-to-make-token-authorized-aes-encrypted-hls-stream-working-in-safari/. 

In order to use the project, you need to update the urlencoded_proxyurl to your site in index.cshtml file. All the manifest manipulation login is in manifestproxycontroller.cs file. You will also need to add your ContentKeyPolicy into the GetTokenAsync() to sign your tokens.

This sample is running live and can be tested at: https://jameelaesahlsproxy.azurewebsites.net/. 

In Summary - You create a proxy to receive the manifest from the streaming endpoint, which it then modifies the manifest playlist so that the URLs contain a token and can then be played back in Safari. This v2 sample contains the authentication system code which will automatically issue an authorized token for each streaming request.  The token will expire in 60mins(1hr).

![image](https://user-images.githubusercontent.com/33047452/150571047-8e848102-0b98-4fd2-b560-808a12815c5d.png)


