<h1>Windows Azure Media Services Verification Tests</h1>
<p>This set of test allows you to verify Windows Azure Media Services .Net SDK functionality using your existing Windows Azure Media Services account</p>
<p>In order to execute test you need to modify app.config file and provide following information</p>

<pre><code>

&lt;add key="ClientSecret"  value="PUT_YOUR_ACCOUNT_PRIMARY_KEY_HERE"/&gt;
&lt;add key="ClientId" value="PUT_YOUR_ACCOUNT_NAME_HERE"/&gt;
</code>
</pre>

<p><b>Please note</b><br>
Executing test will generate encoding workload in your account which will be charged according to  Windows Azure Media Services pricing model for your account <a href="http://www.windowsazure.com/en-us/pricing/details/media-services/">
http://www.windowsazure.com/en-us/pricing/details/media-services/</a> </p>