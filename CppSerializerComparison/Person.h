#pragma once

#include <string>
#include <ctime>
#include <chrono>
#include <utility>
#include <cereal/cereal.hpp>
#include <cereal/types/vector.hpp>
#include <cereal/types/string.hpp>
#include <cereal/types/chrono.hpp>

using namespace std;

struct Document {
	int Id;
	string Name;
	string Content;
	chrono::time_point<chrono::high_resolution_clock> ExpirationDate;

	Document() : Id(0), Name(""), Content(""), ExpirationDate() { }
	Document(int id, string name, string content, chrono::time_point<chrono::high_resolution_clock> expirationDate) :
		Id(id), Name(name), Content(content), ExpirationDate(expirationDate) { }

	template <class Archive>
	void serialize(Archive & ar)
	{
		ar(CEREAL_NVP(Id), CEREAL_NVP(Name), CEREAL_NVP(Content), CEREAL_NVP(ExpirationDate));
	}
};

struct Person {
	int Age;
	chrono::time_point<chrono::high_resolution_clock> Birthday;
	string Name;
	vector<Document> Documents;

	Person() : Age(0), Birthday(), Name(""), Documents() { }
	Person(int age, chrono::time_point<chrono::high_resolution_clock> birthday, string name, vector<Document> documents) : 
		Age(age), Birthday(birthday), Name(name), Documents(documents) { }

	template <class Archive>
	void serialize(Archive & ar)
	{
		ar(CEREAL_NVP(Age), CEREAL_NVP(Birthday), CEREAL_NVP(Name), CEREAL_NVP(Documents));
	}
};