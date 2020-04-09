import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;
import java.net.HttpURLConnection;
import java.net.URL;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Paths;
import java.util.Collections;
import java.util.Set;
import java.util.concurrent.CompletableFuture;
import java.util.function.Consumer;
import java.util.stream.Stream;

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
import com.microsoft.aad.msal4j.DeviceCode;
import com.microsoft.aad.msal4j.DeviceCodeFlowParameters;
import com.microsoft.aad.msal4j.IAccount;
import com.microsoft.aad.msal4j.IAuthenticationResult;
import com.microsoft.aad.msal4j.ITokenCacheAccessAspect;
import com.microsoft.aad.msal4j.MsalException;
import com.microsoft.aad.msal4j.PublicClientApplication;
import com.microsoft.aad.msal4j.SilentParameters;

public class DeviceCodeFlow {

    private final static String PUBLIC_CLIENT_ID = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxx";
    private final static String AUTHORITY_COMMON = "https://login.microsoftonline.us/xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxx";
    private final static String SP_ONLINE_SCOPE = "https://tenant.sharepoint.com/AllSites.FullControl";
    private final static String SP_SITE_URL = "https://tenant.sharepoint.com/sites/siteUrl";

    public static void main(String args[]) throws Exception {

        // Get access token from Azure Active Directory
        IAuthenticationResult authenticationResult = getAccessToken();
        
        System.out.println("Token acquired: " + authenticationResult.accessToken());
        
        String getSiteTitleUri = SP_SITE_URL + "/_api/web?$select=Title";
        String spJsonData = getSharePointRest(authenticationResult.accessToken(), getSiteTitleUri);
        System.out.println("Get site title JSON response:");
        System.out.println(spJsonData);

        System.out.println("Press any key to exit ...");
        System.in.read();
    }

    private static String readLineByLineJava8(String filePath) 
    {
        StringBuilder contentBuilder = new StringBuilder();
 
        try (Stream<String> stream = Files.lines( Paths.get(filePath), StandardCharsets.UTF_8)) 
        {
            stream.forEach(s -> contentBuilder.append(s).append("\n"));
        }
        catch (IOException e) 
        {
            e.printStackTrace();
        }
 
        return contentBuilder.toString();
    }
    
    private static IAuthenticationResult getAccessToken() throws Exception {
//    	Files.lines(Paths.get("/cache_data/serialized_cache.json")).forEach(System.out::println);
    	
//    	String dataToInitCache = readLineByLineJava8("/cache_data/serialized_cache.json");
    	ITokenCacheAccessAspect persistenceAspect = new TokenCache("");
    	
        PublicClientApplication app = PublicClientApplication
                .builder(PUBLIC_CLIENT_ID)
                .authority(AUTHORITY_COMMON)
                .setTokenCacheAccessAspect(persistenceAspect)
                .build();
		
        Set<IAccount> accountsInTokenCache = app.getAccounts().join();
        
		IAuthenticationResult authenticationResult;
		        if(!accountsInTokenCache.isEmpty()){
		
		            // We select the account that we want to get tokens for. For simplicity, we take the first account
		            // in the token cache. In a production application, you would filter to get the desired account
		            IAccount account = accountsInTokenCache.iterator().next();
		            //If the application has an account in the token cache, we will try to acquire a token silently.
		            authenticationResult = getAccessTokenSilently(app, account);
		        } else {
		            // If token cache is empty, we ask the user to put in their credentials in to the
		            // sign in prompt and consent to the requested permissions.
		            authenticationResult = getAccessTokenByDeviceCodeGrant(app);
		    }

        return authenticationResult;
    }

    private static IAuthenticationResult getAccessTokenSilently(
            PublicClientApplication app,
            IAccount account) {

        IAuthenticationResult result;
        try {

            SilentParameters parameters = SilentParameters
                    .builder(Collections.singleton(SP_ONLINE_SCOPE), account)
                    .build();

            result = app.acquireTokenSilently(parameters).join();

        } catch(Exception ex){

            // If acquiring a token silently failed, lets try acquire token interactively
            if(ex instanceof MsalException){
                return getAccessTokenByDeviceCodeGrant(app);
            }

            System.out.println("Oops! We have an exception of type - " + ex.getClass());
            System.out.println("Exception message - " + ex.getMessage());
            throw new RuntimeException(ex);
        }

        return result;
    }

    private static IAuthenticationResult getAccessTokenByDeviceCodeGrant(PublicClientApplication app) {

        Consumer<DeviceCode> deviceCodeConsumer = (DeviceCode deviceCode) -> System.out.println(deviceCode.message());

        DeviceCodeFlowParameters deviceCodeFlowParameters = DeviceCodeFlowParameters
                .builder(Collections.singleton(SP_ONLINE_SCOPE), deviceCodeConsumer)
                .build();

        CompletableFuture<IAuthenticationResult> future = app.acquireToken(deviceCodeFlowParameters);

        future.handle((res, ex) -> {
            if(ex != null) {
                System.out.println("Oops! We have an exception of type - " + ex.getClass());
                System.out.println("Exception message - " + ex.getMessage());
                throw new RuntimeException(ex);
            }

            return res;
        });

        /***************************************************
          ***** It’s a this point that the app hangs *******
         ***************************************************/
        return future.join();
    }
    
    private static String getSharePointRest(String accessToken, String restUri) throws IOException {
    	URL url = new URL(restUri);
        HttpURLConnection conn = (HttpURLConnection) url.openConnection();

        conn.setRequestMethod("GET");
        conn.setRequestProperty("Authorization", "Bearer " + accessToken);
        conn.setRequestProperty("Accept", "application/json;odata=nometadata");

        int httpResponseCode = conn.getResponseCode();
        if(httpResponseCode == 200) {

            StringBuilder response;
            try(BufferedReader in = new BufferedReader(
                    new InputStreamReader(conn.getInputStream()))){

                String inputLine;
                response = new StringBuilder();
                while (( inputLine = in.readLine()) != null) {
                    response.append(inputLine);
                }
            }
            return response.toString();
        } else {
            return String.format("Connection returned HTTP code: %s with message: %s",
                    httpResponseCode, conn.getResponseMessage());
        }
    }    
} 
