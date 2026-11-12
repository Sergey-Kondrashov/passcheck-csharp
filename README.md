PassCheck (C#)

A small command-line tool written in C# (.NET 8) that does two things:

	•	check: rates the strength of a password (length, character types, rough entropy estimate,
warnings for very common passwords, repeated characters and easy sequences like 1234 or qwer)
	•	generate: creates a random password with a cryptographically secure random number generator

The password you type for check is hidden and is never saved or sent anywhere.

Requirements

	•	.NET 8 SDK

Usage

dotnet run -- check
dotnet run -- generate        # 16 characters
dotnet run -- generate 24     # 24 characters

Example:

Enter a password to check (input is hidden):

Length       : 12
Lowercase    : yes
Uppercase    : yes
Digits       : yes
Symbols      : no
Entropy (est): 71 bits
Rating       : strong

How it works

	•	The entropy estimate is length * log2(pool size), where the pool size depends on which
character types are used (26 lowercase, 26 uppercase, 10 digits, 33 symbols).
	•	Common passwords are capped at a very low value. Repeats and sequences reduce the estimate.
	•	generate uses System.Security.Cryptography.RandomNumberGenerator (not System.Random),
guarantees at least one character from each group and shuffles the result.

This is a learning project. The estimate is rough, so use a password manager for real accounts.

Ideas for further work

	•	Passphrase generator with a larger word list
	•	Check against a local list of leaked passwords
	•	Unit tests
