# DMB0001v4
New Dungeon Master bot written based on Basic (C#) EchoBot in ASP.Net core 2 and .NET 4.6.1.

WIPs:
- Unknowns - Add list of asked and unanswered queries with count in order to extend the responses of bot, where it is the most demanded.
  - Need to add separate messages for normal user and admin
  - Add admin commands to convert unknown to retort
  - Add command to remove the oldest backups (always leave one)
- Questions - Known patterns from JSON file matched with given query ito parse things from a quesry and match with known facts.
  - Add matching with patterns
  - Add extending list of pattrns from admin commands
  - Add admin commands to convert unknown to question
- Images - processing from all available sources and sending images to bot to let the bot analyze the content or change it
  - Add downloading images to resources
  - Add legend file to images

Current TODOs (feel free to contribute):
- Speech - Add Text-to-Speech an Speech-To-Text support in order to let the bot be a real chatter.
- Facts - lists of items with common property, which can be used in question or sentence.
  - Fact Categories - list of categories with types and descriptions
- Phrase analyzing - add choping given phrase to parts and do analysis in order to better fit an answer, if there are more, than one
