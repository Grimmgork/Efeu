use strict;
use JSON;
use HTTP::Tiny;

my $ua = HTTP::Tiny->new( 'verify_SSL' => '0' );
my $data = to_json({
	Type => "A",
	Tag => "Data",
	Payload => "42"
});

foreach (1..100000)
{
	make_request($_)
}

sub make_request {
	my $res = $ua->request(
		'POST' => 'https://localhost:5001/Message',
    {
        headers => {
           'Content-Type' => 'application/json'
        },
        content => $data
    });
	my $n = shift;
	print "request $n\n";
}
