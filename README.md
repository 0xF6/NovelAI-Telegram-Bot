<p align="center">
  <h1  align="center">💎 NovelAI streaming telegram bot service with monetization system 👑</h1>
</p>

<p align="center">
  <a href="#">
    <img height="1024" src="https://user-images.githubusercontent.com/13326808/210182808-bd624eed-2dc8-4766-be4c-75a6871c5faa.png">
  </a>
</p>


## TODO
- [x] Switch engine by command (per user)
- [x] Support local db (litedb or other)
- [x] Support invoices
- [ ] Migrate logger from Console.WriteLine to ILogger from Microsoft.Logging
- [x] Detailed config
- [x] Config per model
- [ ] Ability to disable 'pay to generate'
- [ ] Ability to support extend quenue (with multiple NovelAI accounts)
- [ ] Support authorization by username\password

## Setup 

Install `.NET 8.0`         
Set `TelegramBotToken`, `Nai:AuthToken` (see about authorization), `MainAdministrator` in bin/config.yaml      
Execute `bin/run.ps1` or `bin/run.sh`     
Complete! ✨✨✨    

    
About 💎, its direct of Anlas in your novel ai account (btw, crystal formula calculation does not have 100% accuracy), according to this, distribute between users more carefully    
    

For using payment order set up `Currency` (USD, EUR and etc) and `PaymentProviderToken` (getting in bot father in payment menu)     

## Commands

`/balance`    
- View the balance of your 💎

`/engine`      
- View currrent active engine (per user only)

`/engine nai-diffusion-2`    
- Set active engine as nai-diffusion-2 (per user only)
   
`/p 1girl, beer`   
- Generation of portrait sfw pictures    
- You can also specify @DYN or @SMEA     
 (SMEA version of samplers are modified to perform better at high resolution)    
 (DYN variants of SMEA samplers often lead to more varied output, but may fail at very high resolution)

`/l 1girl, beer`         
- The same thing, only landscape orientation      
 
`/variations 1girl`       
- It will generate 10 variations of your promt

`/enhance`    
- Reply to result, generate a variation with low noise, produce an upscale of 1.5 times

`/wallpaper 1girl`    
- Generation of portrait wallpaper of high resolution (auto enabled SMEA toggle)    

`/img2imgp 1girl, beer`   
- We throw off the image without compression and replay it, provide the promt   
- You can also specify @power:0.2 and @noise:0.5 at the end of the promt      
      
`/pay`    
- Pay the dude your 💎 , we reply to the right dude and write the amount 


## Database

In config:
```yaml
Database:
  Firestore:
    IsActive: false
    ConnectionString: ProjectId://nai
    CredentialPath: 
  LiteDB:
    IsActive: true
    ConnectionString: nai.db
```

By default bot use litedb but you can enable firestore, at the moment it has not been tested sufficiently

## About formulas

```yaml
CrystallFormula: "round((map([step], 1, 50, [minAnlas], [maxAnlas]) + map([quality], 4096, 3145728, 2, 136)) / 2)"
EnhanceFormula: "round(price + (price * 0.6))"
VariationFormula: "price * 3"
SeedFormula: "l(0, pow(2, 32) - 1)"
```

You can set custom formula for calculation price and seed


## About authorization

For now, bot required authToken from devtools        
Do auth in novelai.net, press F12, go to Application/Local Storage/Session and copy value of `auth_token`      

![image](https://github.com/0xF6/NovelAI-Telegram-Bot/assets/13326808/493e96a7-295b-4168-9c40-d3a74577b3bb)
