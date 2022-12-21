# MondayOFF SDK


### Requirements
---
- Faceboook App ID
- Facebook SDK (included in the package)
    - Please refer to https://developers.facebook.com/docs/unity/ for more information about Facebook SDK

### Installation
---
- Add **MondayOFFSDK.unitypackage** to your project.

### Usage
---
1. Select **Facebook > Edit Settings** on the menu
![](https://user-images.githubusercontent.com/46589405/174723567-d4b78998-1ca0-437f-818a-0202f5025d04.png)
1. Add Facebook `App Name` and `App ID` in the inspector and click `Regenerate Android Manifest`
![](https://user-images.githubusercontent.com/46589405/174723569-6f9dd004-4988-45e4-b464-44e01076f191.png)
1. Move to the basic setting page on the Facebook developer page, Scroll down and click on “Add Platform”
![](https://user-images.githubusercontent.com/46589405/174723573-ac55d15f-21fe-48b2-b894-a103b5ac3577.png)
1. Select Android and add Google Play to the platform
![](https://user-images.githubusercontent.com/46589405/174723571-f883560f-4af7-47cb-8405-75c47d6b01b3.png)
![](https://user-images.githubusercontent.com/46589405/174723576-025cb4c3-53d8-4bc5-a079-a7a6c5e369d6.png)
1. Add the application's **Package Name** and **Class name** (`com.facebook.unity.FBUnityDeepLinkingActivity`) from step 2
![](https://user-images.githubusercontent.com/46589405/174723575-2a471a76-4c0c-45a2-a2ef-64ac6d9d68ce.png)

1. Add **Assets/MondayOFF/Prefabs/MondayOFF.prefab** to your starting scene.
    - You can also create MondayOFF Game Object to current working scene by selecting **MondayOFF > Create MondayOFF Game Object** on the menu   
    Don't forget to save the scene!
    #### Note
    - ##### If you are initializing Facebook SDK on your own, select MondayOFF Game Object and uncheck **Also initialize Facebook SDK** from the inspector.

1. Make sure all Android Libraries are resolved when building application
            
---

# ​​How to confirm app ownership
![](https://user-images.githubusercontent.com/46589405/174723565-f81960e3-adcd-4c3e-8944-83b7e5876f81.png)
 When adding a platform to your app, Facebook will ask you to verify the ownership of your app on the platform. **Your game must be uploaded to the Google Play store to do this**. In order to confirm your android app ownership on Google Play, you will need to:

1. Have a **website**, or use one of the following **services to create a website**: https://app-adstxt.dev, https://www.app-ads-txt.com 

    - If you are using a service to create your website, follow the steps below.
        -  Make an account on the website and confirm your email address.
        - Enter your app’s store page URL
        - Enter the following into the field
            ```
                facebook.com, app ID, RESELLER, c3e20eee3f780d68
            ```
            #### *Notice*: Replace *app ID* with your game’s app ID

    - If you are using your own website, follow the steps below
        - Create a **.txt** file named **app-ads**
        - Add the following to your app-ads.txt file:
            ```
                facebook.com, app ID, RESELLER, c3e20eee3f780d68
            ```
            #### *Notice*: Replace *app ID* with your game’s app ID

    - Upload the app-ads.txt file to the root of your website.  
    ![](https://user-images.githubusercontent.com/46589405/174723566-1deae90a-ab18-4a3e-8c54-a68d8467b807.png)

2. **Copy** either the **URL** provided by the service or your own website’s URL and **paste it in the “Website” field** under **Store listing contact details** on **Google Play Console**

    Store Presence > Store Listing > Store listing contact details > Website  

3. You’re done! Facebook usually takes **up to 24 hours** to detect the file. It may take longer than that, so please be patient. 

    If you need more help, you can check out Facebook’s guide [here](https://developers.facebook.com/docs/development/release/mobile-app-verification). 