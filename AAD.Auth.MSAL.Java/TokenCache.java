import java.io.File;
import java.io.FileWriter;
import java.io.IOException;
import java.io.PrintWriter;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Paths;
import java.util.stream.Stream;

import com.microsoft.aad.msal4j.ITokenCacheAccessAspect;
import com.microsoft.aad.msal4j.ITokenCacheAccessContext;

public class TokenCache implements ITokenCacheAccessAspect{
        String data;
        String FileName = "C:\\cache_data\\token.cache.json";

        TokenCache(String data){
            this.data = data;
        }

        @Override
        public void beforeCacheAccess(ITokenCacheAccessContext iTokenCacheAccessContext){
        	String content = "";
        	 
            try
            {
                content = new String ( Files.readAllBytes(Paths.get(FileName) ) );
            } 
            catch (IOException e) 
            {
                e.printStackTrace();
            }
     
            
            System.out.println(content);
            iTokenCacheAccessContext.tokenCache().deserialize(content);
        }

        @Override
        public void afterCacheAccess(ITokenCacheAccessContext iTokenCacheAccessContext) {
            File file = new File(FileName);
            
            // Step #2. Create a file writer object with above file.
            FileWriter fileWriter = null;
			try {
				fileWriter = new FileWriter(file, false);
			} catch (IOException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
			}

            // Step #3. Create a file object with above file writer.
            PrintWriter writer = new PrintWriter(fileWriter);

            // Step #4. Perform write operation.

            data = iTokenCacheAccessContext.tokenCache().serialize();
            writer.println(data);

            // Step #5. free the resources.
            writer.close();
        }
        
        private String readLineByLineJava8(String filePath) 
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
    }