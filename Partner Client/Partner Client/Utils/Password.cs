using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PartnerClient.Utils
{
    // Code nicked & modified from my GCSE
    public static class PasswordHelpers
    {

        public static PasswordStrengthTier Evaulate(string text)
        {
            int score;
            // Will match only if the string contains a character other than these
            Regex regex = new Regex(@"[^!$%^#&*()_=+A-Za-z0-9-]");
            if (string.IsNullOrWhiteSpace(text) || regex.IsMatch(text))
                return PasswordStrengthTier.Invalid;
            else if (text.Length < 8)
                return PasswordStrengthTier.Weak;
            else
            {
                // Initialise variables
                bool containsUpperCase = false;
                bool containsLowerCase = false;
                bool containsNumbers = false;
                bool containsSymbols = false;

                // Initiate score with length of password
                score = text.Length;
                //Debug.WriteLine($"score: {score}");

                // Check for lowercase letters
                if (text.ToUpper() != text)
                {
                    containsLowerCase = true;
                    score += 5;
                    //Debug.WriteLine("Contains Lower");
                    //Debug.WriteLine($"score: {score}");
                }
                // Check for uppercase letters
                if (text.ToLower() != text)
                {
                    containsUpperCase = true;
                    score += 5;
                    //Debug.WriteLine("Contains Upper");
                    //Debug.WriteLine($"score: {score}");
                }
                // Check for numbers using regular expression
                Regex numberRegEx = new Regex("[0-9]");
                if (numberRegEx.IsMatch(text))
                {
                    containsNumbers = true;
                    score += 5;
                    //Debug.WriteLine("Contains Num");
                    //Debug.WriteLine($"score: {score}");
                }
                // Checks to see if there are any character which are not letters or numbers
                Regex symbolRegEx = new Regex("[^A-Za-z0-9]");
                if (symbolRegEx.IsMatch(text))
                {
                    containsSymbols = true;
                    score += 5;
                    //Debug.WriteLine("Contains Sym");
                    //Debug.WriteLine($"score: {score}");
                }

                // If all types of character are present
                if (containsLowerCase && containsUpperCase && containsNumbers && containsSymbols)
                {
                    //Debug.WriteLine("All char. types present");
                    score += 10;
                    //Debug.WriteLine($"score: {score}");
                }

                // If only contains letters
                if (!(containsNumbers || containsSymbols))
                {
                    //Debug.WriteLine("Only contains letters");
                    score -= 5;
                    //Debug.WriteLine($"score: {score}");
                }

                // If only contains numbers
                if (!(containsLowerCase || containsUpperCase || containsSymbols))
                {
                    //Debug.WriteLine("Only contains numbers");
                    score -= 5;
                    //Debug.WriteLine($"score: {score}");
                }

                // If only contains symbols
                if (!(containsLowerCase || containsUpperCase || containsNumbers))
                {
                    //Debug.WriteLine("Only contains symbols");
                    score -= 5;
                    //Debug.WriteLine($"score: {score}");
                }

                // List of List (2D array) to respresent a QUERTY keyboard
                List<List<char>> keyboard = new List<List<char>>() {
                    new List<char>{ 'q', 'w', 'e', 'r', 't', 'y', 'u', 'i', 'o', 'p' },
                    new List<char>{ 'a','s','d','f','g','h','j','k','l' },
                    new List<char>{ 'z','x','c','v','b','n','m' }
                };

                // Create an array for the password text for easy iteration
                char[] passwordArray = text.ToLower().ToCharArray();
                // Do once for each row of the keyboard
                foreach (List<char> keyRow in keyboard)
                {
                    // Do once for each character in the password, apart from the last two
                    for (int charPosition = 0; charPosition < passwordArray.Length - 2; charPosition++)
                    {
                        // Where does the password character fall in the keyboard?
                        int tripletIndex = keyRow.IndexOf(passwordArray[charPosition]);
                        // Do if the key is on this row of the keyboard and if this isn't the last two keys
                        if (tripletIndex >= 0 && tripletIndex < keyRow.Count - 2)
                        {
                            char[] keyboardTriplet =
                            {
                                keyRow[tripletIndex],
                                keyRow[tripletIndex + 1],
                                keyRow[tripletIndex + 2]
                            };  // Create an object to store the key and next two consecutive keys


                            char[] passwordTriplet =
                            {
                                passwordArray[charPosition],
                                passwordArray[charPosition + 1],
                                passwordArray[charPosition + 2],
                            };  // Create an object to store the password character and next two consecutive

                            // Testing Code
                            /*for (int i = 0; i < 3; i++) {
                                //Debug.WriteLine($"Keyboard: {keyboardTriplet[i].ToString()}");
                                //Debug.WriteLine($"Password: {passwordTriplet[i].ToString()}");
                                //Debug.WriteLine($"Equal: {passwordTriplet[i] == keyboardTriplet[i]}");
                            }
                            //Debug.WriteLine("");*/

                            if (keyboardTriplet.SequenceEqual(passwordTriplet))
                            {
                                // Take away 5 points if there is a match

                                score -= 5;

                                // Testing Code
                                //Debug.WriteLine($"Triplet found: {passwordTriplet[0].ToString() + passwordTriplet[1].ToString() + passwordTriplet[2].ToString()}");
                                //Debug.WriteLine($"score: {score}");
                            }
                        }
                    }
                }

                //Debug.WriteLine($"score: {score}");

                return score switch
                {
                    int i when i > 20 => PasswordStrengthTier.Strong,
                    int i when i < 1 => PasswordStrengthTier.Weak,
                    _ => PasswordStrengthTier.Medium
                };
            }
        }

    }

    public enum PasswordStrengthTier
    {
        Invalid,
        Weak,
        Medium,
        Strong
    }

}

