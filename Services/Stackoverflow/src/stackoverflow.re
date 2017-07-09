open Bs_fetch;

type tag = { 
  has_synonyms: bool,
  is_moderator_only: bool,
  is_required: bool,
  count: int,
  name: string,
};

type items = array tag;

type responce = { 
  items: items,
  has_more: bool,
  quota_max: int,
  quota_remaining: int,
};

let tagApi = "https://api.stackexchange.com/2.2/tags?order=desc&sort=popular&site=stackoverflow";

let tags = 
  Js.Promise.(
    fetch tagApi
    |> then_ Response.text
    |> then_ (fun text => print_endline text |> resolve)
  );

/*let api = Js.Promise.(
  fetch "/api/hellos/1"
  |> then_ Response.text
  |> then_ (fun text => print_endline text |> resolve)
);*/