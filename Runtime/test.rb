# Name of behaviour

on NameOfMessage {
		id: 10
		name: name
	} do
	emit Message
	emit Message
	await Message {
		id: 10
		name: 10
		name: 20
	} and Message {
		id: 10
		id: 20
	} do
    let name: 1 + 2
	let name: "name"
	  $ split("asdf")
	  $ where do name:
	    name = 10 end
	  $ map do item:
		name = 10 end

	let time: now 
		$ add_minutes(2)
		$ add_seconds(3)
		$ add_days(4)

	let time_asd: time $ {
		name: 10
		name: 20
		name: $ {
			name: 10
			name: 20
			name: 20
		}
	}

	let name: name $ {
		0: 10
		1: $ [
			1: 20
			3: 30
			3: [1, 3, 4]
		]
	}

	let name: {
		Name: 10
		NameAasdf: 20
		NameA: [20, 20, 10]
	}

	let name be [
		1, 3, 4, 5
	]

	let name be 
		name $ push(10)

	"asdf" case
	  "ada" => 10
	  "adawd" => 10
	  _ => nil 
	end

	var then
		10 else 20

	# some commments
    if (a = 10) do
      emit Message
    else
      emit Message
    end
		for items do
			emit Message
			emit Message
			emit Message
			call Message {
				id: 10 
				name: 10
				name: 10
			} do
				await Message
			end
			await Message do
			end
		end
    await Message { 
			id: 10 
		} do
      emit Message
	  emit Message
	  emit Message
    end
	end
end

emit Message
emit Message
await Message { 
	id: 10 
	name: 10 
} do
	let name be 10
	let name be 20
	if a is 10 do
		emit
		emit
	end
end

# Scope

# Instance

# Definition

# scope ids:
#
# 2
# 2.1
# 2.2.0