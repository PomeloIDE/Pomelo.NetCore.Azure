# Before You Run the Server

__MS guys: I spent a lot of time on authenticating Azure API with management certificate. You'd better note it's AD only now in your old docs.__

0. Get your tenant id, subscription id, client id, secret ready
1. Create a Resource Group with area Japan West, get the id, add role for it
2. Create a Network Security Group with IN:ALLOW/ANY/any and OUT:ALLOW/ANY/any
3. Create a Availability Set