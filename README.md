# Raz Ayubi's Chess game application code review

Original repository's address: https://github.com/ayubix/Chess-Console-Application/tree/main

## Important note: I've made sure all the changes I've done compile, but I haven't tested if they run the same. Please do not implement all these changes in your code directly without first testing if these changes don't mess up the game itself.

# Most important - this does not mean your original code is bad. 
You've created a working application in C# when you're clearly more familiar with Java, and that's no easy task.
Most of the changes I've made are, to be honest, not so important - I mean, they do help with readability and help avoid potential problems,
but the main thing to remember here is that your application is impressive enough even without my suggested changes.

## Now, let's get to it:

I've forked your repository and I'll make some changes in some of the files, so that you can see how I would write them.

The first thing I've noticed was you're using Java conventions instead of C# conventions. 
It's important to follow the conventions according to the language you're developing in, 
since most IDEs will issue compilation warnings about this.
This is important since ideally, in production-grade code, you want zero warnings.
Visual Studio allows you to treat warnings as errors, meaning you can't compile without handling all of them, 
and in production-grade code you should use that option.

Resources about C# conventions can be found in official documentation:

[C# identifier naming rules and conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/identifier-names)  
[Common C# code conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)

### I've started with Piece.cs.

#### Converting to an abstract class
This is the base class for chess pieces, so it's an important part of the application.
However, you never use it directly, only the classes derived from it. 
**This is a classic use-case for an abstract class** - so I've changed it to an `internal abstract class` and changed its constructor from `public` to `protected`.
Though it might seem redundant to change the constructor's access modifier to `protected`, it is in fact an important step, 
since even if accidentally you remove the `abstract` keyword, the class will still be abstract de facto, since you will only be able to initialize it via its derived classes.
This also allowed me to convert the `searchForPossibleMoves` method from `virtual` to `abstract`. 
The key difference between virtual and abstract methods is that a virtual method usually has some code that can be overridden in derived classes,
while abstract methods have no code and **must** be overridden in derived classes. 

#### Make it compatible with C# conventions
I've renamed all the fields to start with an underscore (following naming conventions) which allowed me to remove the (now redundant) `this.` for all of them.
Then, I've renamed all the methods (except getters and setters, you'll see why in the next article) to PascalCase instead of camelCase. 

#### Changing getters and setters to properties
In C#, we use properties (the compiler generates a `get_x` and `set_x` method from them) - 
so I've changed `getProtectedPieces` and `setProtectedPieces` methods to a simple auto-implemented property, which in turn caused changes throughout the code whenever there was a reference to any of these methods.
I haven't changed the other getters and setters, though, just this one. 
Also, I've noticed that `getPossibleMovesCount` and `setPossibleMovesCount` both have zero references, but I didn't delete them just yet because I didn't know if you wanted them for future use. 

#### Adding regions
This is actually a personal preference. Visual Studio allows you to have code regions. In my opinion, this helps a lot in classes where you have a lot of code,
since regions are collapsible.  

#### Changed `String` to `string`
Next, I've changed all occurrences of `String` to `string`. Some developers would argue that it's a coding style choice, but in fact it isn't. 
C# will *always* treat `string` correctly, but `String` is just a class name, and if you happen to have some class named `String` in your namespace, that's the class that will be used.
For more information, read [string vs. String is not a style debate](https://blog.paranoidcoding.org/2019/04/08/string-vs-String-is-not-about-style.html) by Jared Parsons (a Principal Developer Lead on C# Language Team).

#### Changed color from text to an enum
The colors of the pieces are a small, fixed set of known values. 
Moreover, if ever a new color would be introduced, it will fundamentally change the game.
This fits perfectly with compile-time constants, and since an enum is a collection of constants, it makes perfect sense to use an enum for colors.

This task took me a relatively long time since you're using color everywhere in the game.  
This is also a good place to talk about ["magic numbers"](https://en.wikipedia.org/wiki/Magic_number_(programming)) - a term that refers to unnamed compile-time constants that repeat over in the code. 
This is an anti-pattern mainly because it's error-prone and compilers will not check to see if you misspelled a value somewhere in the program.
Using Color.White and Color.Black instead of "White" and "Black" means you leverage both IntelliSense and compiler to help you avoid such mistakes.

#### Removed the name variable from constructors of all pieces
Instead, when calling the (now abstract) base class constructor, provided the name of the piece based on the name of the class - (i.e  `:base(nameof(Queen), Color.Black...)`)
This again reduces the chances of typos and also removes redundant repetition when constructing the pieces in the game class.

#### Removed redundant fields
Visual Studio gave me a warning, saying the field _isMate is assigned but its value is never used - so I've simply removed it.
This kind of thing adds confusion for anyone reading the code as well as noise when compiling it. 
I've noticed it's not the only unused field but this is a review, not a fix. I'll leave the rest of them for you.

#### Added missing access modifiers 
I've noticed in the Game.cs there were some fields without access modifiers.
Though it is true that there are default values for access modifiers (the default for a field is `private`),
it is recommended to either consistently use the defaults or consistently ignore them - 
and since most of the fields did have access modifiers, I've added the `private` access modifier to these fields as well.

#### Changed some fields to constants
In the Game.cs class, you've used fields for the number of rows and columns on a board. 
Since this is a constant value, there really is no need for a variable,
and it's also safer to use a constant than a field (even than a readonly field) - so I've changed them to constants.
There are also other fields in other classes that I suspect can be changed to constants.

## Now let's go over some functionality:

#### Changed settingInitialBoard
I've noticed you're adding some pieces more than once - for instance, the black king was added twice and also the white king.
It got me thinking on how we can make this method better - and I came up with the following:
I've added a private class to the game class and called it PlayerPieces. This class takes in its constructor a color, 
and generates all the pieces of this color. 
I've added two readonly fields - _whitePieces and _blackPieces, and initialized them in the Game constructor.
Then I've renamed your `addPieces` method to `AddPiece` (since it only adds one piece at a time)
and added a new method - `AddPieces`, that takes in an instance of the new `PlayerPieces` class and adds its pieces to the board.

This way, the code is much cleaner and less error-prone, as each player starts with exactly all the pieces they need.
I've also changed the way you're getting the king of each color - now instead of looping over the board each time you want to find the king,
you can simply go to the relevant instance of `PlayerPieces` and use its `King` property.

This also means you don't actually need the _whiteKing and _blackKing fields, you can use _whitePieces.King and _blackPieces.King instead.
However, it's not uncommon to use fields or variables as references for longer, more cumbersome names, so it's up to you to decide if you want to change it.

#### Rewrite piece move methods
I've noticed a lot of code repetition in all the different Move methods you've had,
so I've decided to create a single private method called `Move`, and have all the public methods call it instead.
I've started with a rather cumbersome implementation but ended up with a simple, elegant and most importantly - safe implementation.

I've created a private class called Direction, with a private constructor taking in row and col increments,
and a public static readonly field for each possible direction.
This allowed me to call the new `Move` method like this: `Move(board, moves, protectedSquares, Direction.DownRight)`
which is much more readable and much less error-prone than my initial implementation which was this: `Move(board, moves, protectedSquares, -1, 1)`


## Conclusion:
There is still a lot of work left to make this project a production-grade application - including adding automated tests (unit, integration, and e2e), 
however, since this has already taken most of my morning, I think I can safely stop here.
In all code reviews I've done and received, a document with explanations doesn't usually exist. 
Typically, you'll get a comment on your PR asking for a change.
However, I thought this kind of review will help you much more (and also, it helped me too to consider different ways to implement things).

