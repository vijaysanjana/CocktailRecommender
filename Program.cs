using System;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace CocktailRecommender
{
    class Cocktail
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string ImageUrl { get; set; }
        public string Instructions { get; set; }
        public List<string> Ingredients { get; set; }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            bool again = true;
            while (again != false)
            {
                Console.WriteLine("Welcome to the Cocktail Recommender!");
                Console.WriteLine("How would you like to search for a cocktail?");
                Console.WriteLine("1. Search by name");
                Console.WriteLine("2. List all cocktails by first letter");
                Console.WriteLine("3. Search by ingredient");
                Console.WriteLine("4. Lookup a random cocktail");

                int userInput;
                while (!int.TryParse(Console.ReadLine(), out userInput) || userInput < 1 || userInput > 4)
                {
                    Console.WriteLine("Invalid input, please try again.");
                }

                string searchUrl = "";

                switch (userInput)
                {
                    case 1:
                        Console.WriteLine("Please enter the name of the cocktail you would like to search for:");
                        string cocktailName = Console.ReadLine();
                        searchUrl = $"https://www.thecocktaildb.com/api/json/v1/1/search.php?s={cocktailName}";
                        break;
                    case 2:
                        Console.WriteLine(
                            "Please enter the first letter of the cocktail name you would like to search for:");
                        string firstLetter = Console.ReadLine();
                        searchUrl = $"https://www.thecocktaildb.com/api/json/v1/1/search.php?f={firstLetter}";
                        break;
                    case 3:
                        Console.WriteLine("Please enter the ingredient you would like to search for:");
                        string ingredient = Console.ReadLine();
                        searchUrl = $"https://www.thecocktaildb.com/api/json/v1/1/filter.php?i={ingredient}";
                        break;
                    case 4:
                        searchUrl = $"https://www.thecocktaildb.com/api/json/v1/1/random.php";
                        break;
                    default:
                        Console.WriteLine("Invalid input");
                        break;
                }

                List<Cocktail> cocktails = await GetCocktailsAsync(searchUrl);

                if (cocktails.Count > 0)
                {
                    Console.WriteLine($"Here are the {cocktails.Count} cocktails that match your search:");

                    for (int i = 0; i < cocktails.Count; i++)
                    {
                        Console.WriteLine($"{i + 1}. {cocktails[i].Name}");
                    }

                    Console.WriteLine(
                        "Enter the number of the cocktail you would like to see the recipe for, or 0 to go back to the search:");
                    int cocktailIndex;
                    while (!int.TryParse(Console.ReadLine(), out cocktailIndex) || cocktailIndex < 0 ||
                           cocktailIndex > cocktails.Count)
                    {
                        Console.WriteLine("Invalid input, please try again.");
                    }

                    if (cocktailIndex > 0)
                    {
                        Cocktail selectedCocktail = cocktails[cocktailIndex - 1];
                        Console.WriteLine($"Name: {selectedCocktail.Name}");
                        Console.WriteLine($"Category: {selectedCocktail.Category}");
                        Console.WriteLine($"Instructions: {selectedCocktail.Instructions}");

                        Console.WriteLine("Ingredients:");
                        foreach (var ingredient in selectedCocktail.Ingredients)
                        {
                            Console.WriteLine($"- {ingredient}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No cocktails found matching your search.");
                }

                Console.WriteLine("Would you like to search again? (y/n)");
                string userAgain = Console.ReadLine();
                if (userAgain == "y")
                {
                    again = true;
                }
                else
                {
                    again = false;
                }
            }
        }

        static async Task<List<Cocktail>> GetCocktailsAsync(string url)
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(url);

            List<Cocktail> cocktails = new List<Cocktail>();

            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                dynamic data = JsonConvert.DeserializeObject(responseContent);

                foreach (dynamic drink in data.drinks)
                {
                    Cocktail cocktail = new Cocktail
                    {
                        Id = drink.idDrink,
                        Name = drink.strDrink,
                        Category = drink.strCategory,
                        ImageUrl = drink.strDrinkThumb,
                        Instructions = drink.strInstructions,
                        Ingredients = new List<string>()
                    };

                    // Extract ingredients and measurements
                    for (int i = 1; i <= 15; i++)
                    {
                        string ingredient = drink[$"strIngredient{i}"];
                        string measurement = drink[$"strMeasure{i}"];

                        if (!string.IsNullOrEmpty(ingredient))
                        {
                            if (!string.IsNullOrEmpty(measurement))
                            {
                                ingredient = $"{measurement} {ingredient}";
                            }

                            cocktail.Ingredients.Add(ingredient);
                        }
                        else
                        {
                            // No more ingredients
                            break;
                        }
                    }

                    cocktails.Add(cocktail);
                }
            }

            return cocktails;
        }
    }
}