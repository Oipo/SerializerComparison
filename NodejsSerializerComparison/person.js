exports.Document = class Document {
    constructor(id, name, content, expirationDate) {
        this.id = id;
        this.name = name;
        this.content = content;
        this.expirationDate = expirationDate;
    }
}

exports.Person = class Person {
    constructor(age, birthday, name, documents) {
        this.age = age;
        this.birthday = birthday;
        this.name = name;
        this.documents = documents;
    }
}