# Spam

I am a long-time [SpamCop](https://www.spamcop.net/) user. While I haven't seen a noticeable drop in the spam I've received, I have gotten a few messages from ISPs that they will be stopping the spam at their end.

When processing spam, I followed this general process:

1. Navigate to my `Spam` folder.
2. Select all the emails.
3. Forward as an attachment to my SpamCop address.
4. Delete the selected emails.
5. Open the SpamCop report email.
6. Open all the links in a new browser tab, one for each spam.
7. Click the *Send Spam Report(s) Now* button.
8. Close the browser tab.
9. Delete the SpamCop email.

I had wanted to write a program using Gmail's API to send/receive emails. I started writing a prototype program when the ChatGPT craze started and thought this might be a good challenge for it. It took a lot of prodding and prompting, but it returned a workable result.

I might convert this to a Windows service. But, for now, it runs just fine as a scheduled task.