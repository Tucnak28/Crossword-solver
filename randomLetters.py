import itertools

def generate_combinations(pattern):
    letters = 'abcdefghijklmnoprstuvyz'
    combinations = itertools.product(letters, repeat=pattern.count('.'))
    with open('combinations.txt', 'w') as file:
        for combo in combinations:
            index = 0
            result = ''
            for char in pattern:
                if char == '.':
                    result += combo[index]
                    index += 1
                else:
                    result += char
            if contains_duplicate_letters(result):
                continue
            file.write(result + '\n')

def contains_duplicate_letters(word):
    for i in range(len(word) - 1):
        if word[i] == word[i + 1]:
            return True
    return False

generate_combinations('at se vam valce ....toci')
print("Combinations without adjacent identical letters have been logged into combinations.txt")
