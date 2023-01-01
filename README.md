<p align="center">
  <h1  align="center">💎 NovelAI streaming telegram bot service with monetization system 👑</h1>
</p>

<p align="center">
  <a href="#">
    <img height="1024" src="https://user-images.githubusercontent.com/13326808/210182808-bd624eed-2dc8-4766-be4c-75a6871c5faa.png">
  </a>
</p>



## Setup 

Install `.NET 7.0`      
Set `GOOGLE_APPLICATION_CREDENTIALS`, `FIRESTORE_PROJECT_ID` and `TELEGRAM_BOT_TOKEN` env     
execute `dotnet run`  
Complete! ✨✨✨

    
About 👑, its direct of Anlas in your novel ai account, according to this, distribute between users more carefully    
    

For using payment order set up `TELEGRAM_PAYMENT_CURRENCY` (USD, EUR and etc) and `TELEGRAM_PAYMENT_PROVIDER_TOKEN` (getting in bot father in payment menu)



### Configuration database (optional, you can use local json file, but adapter need implement)      

Create google cloud account and set up Cloud Firestore      
Create collection `nai` in root of firestore      
Add document `chats` and insert array with `data` name, its array of telegram IDs chat (private or public)      
Add document `novalai` and insert field `Token` with your NovelAI account JWT token (expires of 2 week, and getting from Network Tab when execute promt in ImageGeneration tab in novelai.net)        
Add document `owner` and insert field `id`, its id of your telegram account     


For auth with firebase, goo gle to getting ServiceAccount JSON key      

## Commands

`/balance`    
- View the balance of your 👑 and 💎

`/sfwp 1girl, beer`   
- Generation of portrait sfw pictures, eats ~6 💎
`/sfwl 1girl, beer`     
- The same thing, only landscape orientation  
- Also eats ~6 💎    
  
`/nsfwp 1girl, nipples, naked, nude, etc`     
- NSFW image generation, portrait orientation, price ~18 💎   
`/nsfwl 1girl, nipples, naked, nude, etc`       
- The same thing, only landscape orientation          
    
`/variations 1girl`   
- It will generate 10 variations of your promt, the price is 450 💎     
`/enhance`    
- Reply to result, generate a variation with low noise, produce an upscale of 1.5 times, the price is 14 👑 and 36 💎   
    
`/img2imgp 1girl, beer`   
- We throw off the image without compression and replay it, provide the promt   
- You can also specify power:0.2 and noise:0.5 at the end of the promt  
- Price 13 💎 and 11 👑  
      
`/pay_crown`    
- Pay the dude your 👑 👑 👑 , we reply to the right dude and write the amount 
`/pay_crystal`    
- The same thing, but we pay 💎 💎 💎 

`/exhange 200`    
- Exchange 200 crystals 💎 for 10 crowns 👑  
