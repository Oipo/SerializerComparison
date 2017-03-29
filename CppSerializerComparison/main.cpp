#define WIN32_LEAN_AND_MEAN
#define NOMINMAX
#include <windows.h>
#include <iostream>
#include <functional>
#include <chrono>
#include <numeric>

#include <cereal/archives/binary.hpp>
#include <cereal/archives/json.hpp>
#include <cereal/archives/xml.hpp>

#define ELPP_NO_DEFAULT_LOG_FILE
#include "easylogging++.h"
#include "person.h"
#include "person.pb.h"

INITIALIZE_EASYLOGGINGPP;

const int repititions = 250;

using namespace std;

long long runTest(function<void(void)> action) {
	auto startTime = chrono::high_resolution_clock::now();
	action();
	auto endTime = chrono::high_resolution_clock::now();
	return chrono::duration_cast<std::chrono::microseconds>(endTime - startTime).count();
}

void printMeasurements(string name, vector<long long> measurements) {
	auto min = *min_element(begin(measurements), end(measurements));
	auto max = *max_element(begin(measurements), end(measurements));
	auto avg = accumulate(begin(measurements), end(measurements), 0ll) / measurements.size();

	cout << name << " - min " << min << " µs - max " << max << " µs - avg " << avg << " µs" << endl;
}

Person createPerson() {
	vector<Document> documents;
	for (int i = 0; i < 1000; i++) {
		documents.emplace_back(i, "License" + to_string(i), "abcdefghijklmnopqrstuvwxyzüäçéèß", chrono::high_resolution_clock::now() + chrono::hours(i));
	}

	return Person(123, chrono::high_resolution_clock::now(), "John Doe", documents);
}

SerializerComparison::PersonProtobuf createPersonProtobuf() {
	SerializerComparison::PersonProtobuf person;
	person.set_age(123);
	person.set_name("John Doe"s);

	for (int i = 0; i < 1000; i++) {
		SerializerComparison::DocumentProtobuf *document = person.add_documents();
		document->set_id(i);
		document->set_name("License"s + to_string(i));
		document->set_content("abcdefghijklmnopqrstuvwxyzüäçéèß"s);
		google::protobuf::Timestamp *time = new google::protobuf::Timestamp();
		auto chronoTime = chrono::high_resolution_clock::now() + chrono::hours(i);
		time->set_seconds(chrono::duration_cast<chrono::seconds>(chronoTime.time_since_epoch()).count());
		document->set_allocated_expirationdate(time);
	}

	return person;
}

void logMeasurements(string name, vector<long long> measurements) {
	stringstream measurementsStream;
	copy(begin(measurements), end(measurements), ostream_iterator<int>(measurementsStream, ":"));
	// The three pipes are there for BoxPlotter to pick up the results properly
	string joined = measurementsStream.str();
	LOG(DEBUG) << name << ": " << joined.substr(0, joined.size() - 1);
}

int main()
{
	ios_base::sync_with_stdio(false);
	setlocale(LC_ALL, "");

	GOOGLE_PROTOBUF_VERIFY_VERSION;

	HANDLE process = GetCurrentProcess();
	if (!SetProcessAffinityMask(process, 1)) {
		cout << "Couldn't set processor affinity" << endl;
	}

	if (!SetPriorityClass(process, HIGH_PRIORITY_CLASS)) {
		cout << "Couldn't set process priority" << endl;
	}

	el::Configurations defaultConf;
	defaultConf.setToDefault();
	defaultConf.setGlobally(el::ConfigurationType::Format, "|%datetime|%level|%msg");
	defaultConf.setGlobally(el::ConfigurationType::Filename, "measurements.txt");
	defaultConf.setGlobally(el::ConfigurationType::ToStandardOutput, "false");
	el::Loggers::reconfigureLogger("default", defaultConf);

	vector<long long> inputMeasurements;
	vector<long long> outputMeasurements;
	Person person = createPerson();
	auto personProtobuf = createPersonProtobuf();

	LOG(INFO) << "Starting measurements";

	for (int i = 0; i < repititions; i++) {
		stringstream ss; // any stream can be used
		{
			cereal::BinaryOutputArchive oarchive(ss); // Create an output archive
			outputMeasurements.push_back(runTest([&]() {
				oarchive(person);
			}));
		}
		Person newperson;
		{
			cereal::BinaryInputArchive iarchive(ss); // Create an input archive
			inputMeasurements.push_back(runTest([&]() {
				iarchive(newperson);
			}));
		}
	}

	printMeasurements("Cereal Binary Serialize", outputMeasurements);
	printMeasurements("Cereal Binary Deserialize", inputMeasurements);

	logMeasurements("Cereal Binary Serialize", outputMeasurements);
	logMeasurements("Cereal Binary Deserialize", inputMeasurements);

	outputMeasurements.clear();
	inputMeasurements.clear();

	for (int i = 0; i < repititions; i++) {
		stringstream ss; // any stream can be used
		{
			cereal::JSONOutputArchive oarchive(ss); // Create an output archive
			outputMeasurements.push_back(runTest([&]() {
				oarchive(person);
			}));
		}
		Person newperson;
		{
			cereal::JSONInputArchive iarchive(ss); // Create an input archive
			inputMeasurements.push_back(runTest([&]() {
				iarchive(newperson);
			}));
		}
	}

	printMeasurements("Cereal Json Serialize", outputMeasurements);
	printMeasurements("Cereal Json Deserialize", inputMeasurements);

	logMeasurements("Cereal Json Serialize", outputMeasurements);
	logMeasurements("Cereal Json Deserialize", inputMeasurements);

	outputMeasurements.clear();
	inputMeasurements.clear();

	for (int i = 0; i < repititions; i++) {
		stringstream ss; // any stream can be used
		{
			cereal::XMLOutputArchive oarchive(ss); // Create an output archive
			outputMeasurements.push_back(runTest([&]() {
				oarchive(person);
			}));
		}
		Person newperson;
		{
			cereal::XMLInputArchive iarchive(ss); // Create an input archive
			inputMeasurements.push_back(runTest([&]() {
				iarchive(newperson);
			}));
		}
	}

	printMeasurements("Cereal XML Serialize", outputMeasurements);
	printMeasurements("Cereal XML Deserialize", inputMeasurements);

	logMeasurements("Cereal XML Serialize", outputMeasurements);
	logMeasurements("Cereal XML Deserialize", inputMeasurements);

	outputMeasurements.clear();
	inputMeasurements.clear();

	for (int i = 0; i < repititions; i++) {
		stringstream ss; // any stream can be used
		{
			outputMeasurements.push_back(runTest([&]() {
				personProtobuf.SerializeToOstream(&ss);
			}));
		}
		SerializerComparison::PersonProtobuf newPerson;
		{
			inputMeasurements.push_back(runTest([&]() {
				newPerson.ParseFromIstream(&ss);
			}));
		}
	}

	printMeasurements("Protobuf Serialize", outputMeasurements);
	printMeasurements("Protobuf Deserialize", inputMeasurements);

	logMeasurements("Protobuf Serialize", outputMeasurements);
	logMeasurements("Protobuf Deserialize", inputMeasurements);

	LOG(INFO) << "Stopping measurements";

	system("pause");

	return 0;
}