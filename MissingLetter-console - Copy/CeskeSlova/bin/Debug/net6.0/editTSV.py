import csv
from unidecode import unidecode

# Seznam souborů
files = [
    "sorted_words_by_frequency.tsv",
]

# Načtení slov z jednotlivých souborů
words = {}  # Použijeme slovník pro ukládání slov spolu s informacemi o ranku a frekvenci
for file_name in files:
    with open(file_name, 'r', encoding='utf-8') as file:
        reader = csv.reader(file, delimiter='\t')
        for row in reader:
            word = unidecode(row[1]).lower()
            if word not in words:
                words[word] = (row[0], row[2])  # Uložení ranku a frekvence spolu se slovem

# Seřazení slov podle délky od největšího k nejmenšímu
sorted_words = sorted(words.items(), key=lambda x: -len(x[0]))

# Uložení seřazených slov do nového .tsv souboru
output_file = "sorted_words.tsv"
with open(output_file, 'w', encoding='utf-8', newline='') as file:
    writer = csv.writer(file, delimiter='\t')
    for word, (rank, frequency) in sorted_words:
        writer.writerow([rank, word, frequency])  # Zápis ranku, slova a frekvence do souboru

print("Slova byla seřazena od největší délky k nejmenší a uložena do souboru")
