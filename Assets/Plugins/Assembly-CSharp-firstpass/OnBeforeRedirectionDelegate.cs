using System;

public delegate bool OnBeforeRedirectionDelegate(HTTPRequest request, HTTPResponse response, Uri redirectUri);
