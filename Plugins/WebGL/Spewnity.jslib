var SpewnityPlugin =
{
    Prompt:function(prompt, defaultInput)
    {
    	var response = window.prompt(Pointer_stringify(prompt), Pointer_stringify(defaultInput));
    	if(response == "")
    		return null;
        var buffer = _malloc(lengthBytesUTF8(response) + 1);
        writeStringToMemory(response, buffer);
        return buffer;    	
    },

    Alert:function(msg)
    {
    	window.alert(Pointer_stringify(msg));
    }

};
mergeInto(LibraryManager.library, SpewnityPlugin);
