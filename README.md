# SerializerComparison

After having read a couple of benchmark results on the internet, one of which claimed that NetJSON in C# was faster than JIL in C#, which was contrary to my experience, I set out to create a more reliable microbenchmark of various C# serializers. I ended up including a couple of C++ and JS serializers as well.

# Building

All sources provided are only made sure to work on Visual Studio 2017, Windows 10 x64. Any other configuration and you're on your own. Sorry.

Simply open the sln file and you should be good to go. The C++ project has all you need already in it, including the protobuf library.

# Using the BoxPlotter project

BoxPlotter parses the outputted measurements.txt of each type of project. However, the C# benchmark outputs doubles as (example) "3,4" while the NodeJS benchmark outputs it as (example) "3.4". BoxPlotter can only handle (example) "3.4" and for this reason, before having BoxPlotter parse the results, simply do a find & replace in the measurements.txt file of "," to ".".