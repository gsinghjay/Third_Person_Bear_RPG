using Yarn.Unity;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

public static class DialogueRunnerExtensions
{
    public static bool IsCommandRegistered(this DialogueRunner dialogueRunner, string commandName)
    {
        try
        {
            // Use reflection to access the private Commands dictionary
            var commandsField = typeof(DialogueRunner).GetField("commands", 
                BindingFlags.NonPublic | BindingFlags.Instance);
                
            if (commandsField != null)
            {
                var commands = commandsField.GetValue(dialogueRunner) as IDictionary<string, object>;
                return commands != null && commands.ContainsKey(commandName);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Error checking command registration: {e.Message}");
        }
        
        return false;
    }
}