# Content-Addressed JSON DAG

## 1. Core model

The system stores an immutable JSON DAG in SQL.

Each node is identified by the cryptographic hash of its canonical binary representation:

```sql
CREATE TABLE node (
    hash    BYTEA PRIMARY KEY,
    payload BYTEA NOT NULL
);
```

- `hash` is the node's content identity.
- `payload` is the canonical serialized representation.
- Identical content produces the same hash and is stored only once.
- Nodes are immutable.

The canonical serialization must define all semantic details, including:
- object key representation/order
- array ordering
- scalar representation
- number normalization
- string encoding
- references to child nodes

Do not hash arbitrary JSON text; hash the canonical binary representation.

---

## 2. Graph topology

Graph relationships are stored separately from the payload:

```sql
CREATE TABLE reference (
    source_hash BYTEA NOT NULL
        REFERENCES node(hash) ON DELETE CASCADE,

    target_hash BYTEA NOT NULL
        REFERENCES node(hash),

    PRIMARY KEY (source_hash, target_hash)
);

CREATE INDEX reference_source_idx
    ON reference(source_hash);

CREATE INDEX reference_target_idx
    ON reference(target_hash);
```

The relation has deliberately minimal semantics:

> Node A references node B.

It does **not** describe whether the reference is:
- an object property
- an array element
- ordered
- associated with a particular key

Those semantics belong to `payload`.

`reference` is a graph/topology index primarily used for reachability and garbage collection.

If an array contains the same child multiple times, `reference` still only needs one edge because multiplicity/order are retained in the payload.

---

## 3. Root references

There is a small, fixed set of known application tables that hold roots.

For example:

```sql
CREATE TABLE document (
    id        BIGINT PRIMARY KEY,
    node_hash BYTEA NOT NULL REFERENCES node(hash)
);

CREATE TABLE user_entity (
    id        BIGINT PRIMARY KEY,
    node_hash BYTEA NOT NULL REFERENCES node(hash)
);

CREATE TABLE session (
    id        BIGINT PRIMARY KEY,
    node_hash BYTEA NOT NULL REFERENCES node(hash)
);
```

Do not introduce a generic `root` table unless the number/type of root sources becomes dynamic.

A node is live if it is reachable from **any** root in any of these known tables.

---

## 4. Loading / deserialization

To load a complete DAG, recursively find all nodes reachable from the requested root:

```sql
WITH RECURSIVE reachable(hash) AS (
    SELECT node_hash
    FROM document
    WHERE id = $1

    UNION

    SELECT r.target_hash
    FROM reference r
    JOIN reachable x
      ON r.source_hash = x.hash
)
SELECT n.hash, n.payload
FROM reachable x
JOIN node n
  ON n.hash = x.hash;
```

Use `UNION`, not `UNION ALL`, so shared nodes are visited only once.

The application can then:

1. Put all loaded nodes into a `Map<hash, Node>`.
2. Deserialize nodes from their payload.
3. Resolve child hashes through the map.
4. Reuse already-created objects for shared hashes.

This preserves DAG sharing in memory and avoids N+1 database queries.

The SQL relation does not need an ordinal. Array/object ordering is already encoded in the payload, and any global traversal order would be root-dependent anyway.

---

## 5. Garbage collection

Use simple **root-based reachability tracing**.

A node is garbage when it cannot be reached from any known root.

Conceptually:

```text
known application roots
        ↓
   reachable nodes
        ↓
      keep

everything else
        ↓
      delete
```

The GC query begins with the union of all known root columns:

```sql
WITH RECURSIVE reachable(hash) AS (
    SELECT node_hash FROM document
    UNION
    SELECT node_hash FROM user_entity
    UNION
    SELECT node_hash FROM session

    UNION

    SELECT r.target_hash
    FROM reference r
    JOIN reachable x
      ON r.source_hash = x.hash
)
SELECT n.hash
FROM node n
LEFT JOIN reachable r
  ON r.hash = n.hash
WHERE r.hash IS NULL;
```

Unreachable nodes can then be deleted, preferably in batches for large datasets.

Deleting a node automatically removes its outgoing `reference` rows because `source_hash` uses `ON DELETE CASCADE`.

Do **not** cascade deletion from an application root to `node`. Removing a root only makes the node potentially collectible; it does not mean the node should immediately be deleted.

---

## 6. Overall architecture

```text
Application tables
(document, user_entity, session, ...)
             │
             │ node_hash
             ▼
      ┌─────────────────┐
      │      node       │
      │ hash + payload  │
      └─────────────────┘
             │
             │ topology
             ▼
      ┌─────────────────┐
      │    reference    │
      │ source → target │
      └─────────────────┘
             │
             ▼
       Reachability GC
```

### Design principles

- **Content-addressed:** hash is the node identity.
- **Immutable:** nodes are never modified after creation.
- **Deduplicated:** identical content is stored once.
- **Payload owns semantics:** serialization defines JSON structure.
- **Reference owns topology:** edges only say that one node references another.
- **Known tables own roots:** no generic root abstraction is necessary.
- **GC is tracing-based:** keep everything reachable from any root.
- **No edge ordering:** traversal/deserialization order is not a property of the graph index.
- **Application-side reconstruction:** load the reachable graph once, then resolve references in memory.