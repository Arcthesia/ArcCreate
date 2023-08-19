## Translating for ArcCreate

If you want to submit your translation to this repository, you'll first want to login to your GitHub account. If you don't have an account yet, [create a new one](https://github.com).

Next, you'll need the locale code for your language. Head over to the [list of locale code identifiers here](https://www.science.co.il/language/Locale-codes.php), and find the row with the name of your language. The code you want will be in the third column named "LCID string".

> For example: The code for English (United States) would be `en-us`.

This code will be used to identify the file name you'll need to modify/create. Namely, you'll need to head to the locale folder in `/Assets/StreamingAssets/Locales/`. You can either work on translation directly on the GitHub webpage, in which case head over to [this link](/Assets/StreamingAssets/Locales/), or you can clone the repository to your local device, and make changes necessary there. I will assume you know what you're doing if you went with the second method and will only explain the first method.

Now either create a new file named `(your language code).yml`, or open the file if it exists already.

#### Modifying an existing translation

If the file already exists, then someone else has already started working on the translation, and you can contribute by editing this further. Open the file, then click on the edit button (with a pencil as the icon) on the top right. You're now ready to start making changes.

> Note: For every new build, missing entries in all locale files are filled in with the default english text from [en-us.yml](/Assets/StreamingAssets/Locales/). Feel free to update the translation by filling these in whenever a new build is released.

#### Creating a new translation

If the file doesn't exist yet, you need to create it yourself.

First, head over to the default locale file [en-us.yml](/Assets/StreamingAssets/Locales/), and copy all of its content. Then click on `Add File > Create new file`, name the file with the correct name mentioned above, and paste everything you just copied into this file.

Next, in order for ArcCreate to recognize your new locale file, you must add it to the [locale_list.yml](/Assets/StreamingAssets/). Follow the example and add a new entry in the following format:
```yml
- Id: your-locale-file
  CodeName: CodeNameForLanguage
  LocalName: Language Name
```

Where:
- `Id` is what your locale file is named i.e the LCID string listed on the [locale code identifiers list](https://www.science.co.il/language/Locale-codes.php).
- `CodeName` is the name of your language as listed in the [list of system languages provided by Unity](https://docs.unity3d.com/ScriptReference/SystemLanguage.html).
- `LocalName` is what your language will be displayed as in game.

After creating the file and modifying the locale list, you're now ready to start translating.

#### Submitting changes

After you're done with the translation, your next step is to submit your changes. Press the `Propose changes` or `Propose new file` button at the bottom. Press `Create pull request` twice, and you have successfully submitted your changes. All that's left is to wait until they're approved and merged into the repository.

#### Testing your translation

You'll probably want to test out how your translation will look first before submitting it. There are multiple ways to do this:

1. Within UnityEditor: you will need to install Unity for this. Clone the project to your local device, and edit the locale files similarly to the method described above. You can now test the translation live within the Editor.

2. With a Windows/Mac/Linux build: you will need to navigate to locale folder located within your installation of ArcCreate:

   - On Windows/Linux: it's `(Path to your installation)/ArcCreate_Data/StreamingAssets/Locales/`
   - On Mac: it's `(Path to your installation)/ArcCreate.app/Contents/Resources/Data/StreamingAssets/Locales/`

   You can modify a locale file, or create a new locale file here, and reload it within ArcCreate to see the result.

Unfortunately there is no easy way to test on Android and iOS. You will need to rebuild the application, at which point you might as well use method 1 to test your translation.

#### General tip for translation

###### YAML Format

The locale file uses the `.yml` file format, which is a configuration file format designed to be human-readable and easily editable.

You'll want to make sure your locale file follow the specification of `.yml`. If you just mimic the default locale file things should be fine, but you can paste the file content into [an online YAML editor like this one](https://codebeautify.org/yaml-editor-online) to make sure there's no syntax error.

###### Common YAML syntax error

Using VSCode, or the YAML editor mentioned above, or any other text editor that can syntax highlight YAML format will make it harder to encounter issues with editing YAML file.

The following is a few common syntax error that you might encounter, and how to fix it.

1. Beginning a string with `{`, or `}` is not allowed (placing thema anywhere else is okay). Enclose the sentence with a double quote `"`, or a single quote `'` if your sentence already include a double quote.

   *Example:*
   - Bad: `Key: {0} this is a sentence` (begins with `{`)
   - Good: `Key: "{0} this is a sentence"` (enclosed properly with `"`)
   - Best: `Key: '{0} this is a sentence'` (`'` always work, use it so you don't confuse yourself)

   *Example:*
   - Bad: `Key: {0} this sentence uses "quote"` (begins with `{`)
   - Bad: `Key: "{0} this sentence uses "quote""` (YAML doesn't understand nested `""`)
   - Good: `Key: '{0} this sentence uses "quote"'`

###### Text content

1. Sometime you'll see text content in the form of `{argument}`. These will be replaced when displayed within the application.

   > For example, `Error: {Message}` will have `{Message}` be replaced with an error message when displayed.

   You can reorder these brackets around, but do not modify its content. An unrecognized argument will be replaced with `"???"` when displayed.


2. You can use Unity Rich Text syntax within your text content. Refer to [Unity's documentation here](https://docs.unity3d.com/Packages/com.unity.textmeshpro@4.0/manual/RichText.html).

   An example of a good way to use this would be using the `<size>` tag to scale down texts that would otherwise be too long.

   *Example:*
   - `This text is very long` -> `<size=10>This text is very long</size>`

#### Translation credits

I very much appreciate any help with translation. Therefore you're free to modify the credit at `Assets/StreamingAssets/credit.txt` to include your name, (or if you don't know how to, contact me and let me know and I can do it for you).

If you have any issues, feel free to let me know on Discord (0thElement#2457, though the tag number might change) or any other means.
