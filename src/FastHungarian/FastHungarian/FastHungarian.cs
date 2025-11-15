using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FastHungarian;

/// <summary>
/// Result of maximum weight matching.
/// </summary>
public record Matching(int[] LeftPairs, int[] RightPairs, int WeightSum);

/// <summary>
/// Fast Hungarian Algorithm based on Kwok's algorithm.
/// </summary>
public static class FastHungarian
{
    private static List<(int vertex, int weight)> RetainTopK(List<(int vertex, int weight)> edges, int k)
    {
        if (edges.Count <= k) return edges;

        QuickSelectTopK(edges, 0, edges.Count - 1, k);
        return edges.GetRange(0, k);
    }
    
    private static void QuickSelectTopK(List<(int vertex, int weight)> edges, int left, int right, int k)
    {
        while (left < right)
        {
            int pivotIndex = MedianOfThree(edges, left, right);
            pivotIndex = Partition(edges, left, right, pivotIndex);

            if (pivotIndex == k - 1)
            {
                return;
            }

            if (pivotIndex < k - 1)
            {
                left = pivotIndex + 1;
            }
            else
            {
                right = pivotIndex - 1;
            }
        }
    }
    
    private static int Partition(List<(int vertex, int weight)> edges, int left, int right, int pivotIndex)
    {
        int pivotWeight = edges[pivotIndex].weight;
        
        (edges[pivotIndex], edges[right]) = (edges[right], edges[pivotIndex]);
        
        int storeIndex = left;

        for (int i = left; i < right; i++)
        {
            if (edges[i].weight > pivotWeight)
            {
                (edges[i], edges[storeIndex]) = (edges[storeIndex], edges[i]);
                storeIndex++;
            }
        }
        
        (edges[storeIndex], edges[right]) = (edges[right], edges[storeIndex]);
        
        return storeIndex;
    }
    
    private static int MedianOfThree(List<(int vertex, int weight)> edges, int left, int right)
    {
        int mid = left + (right - left) / 2;
        
        int leftWeight = edges[left].weight;
        int midWeight = edges[mid].weight;
        int rightWeight = edges[right].weight;
        
        if (leftWeight >= midWeight != leftWeight >= rightWeight)
            return left;
        return midWeight >= leftWeight != midWeight >= rightWeight ? mid : right;
    }
    
    /// <summary>
    /// Computes the maximum weight matching using modified Kwok's algorithm.
    /// </summary>
    /// <param name="leftCount">Size of vertices in L (|L|).</param>
    /// <param name="rightCount">Size of vertices in R (|R|).</param>
    /// <param name="adjacencyList">The adjacency list where each list contains (vertex, weight) pairs.</param>
    /// <returns>Matching result with left pairs, right pairs, and total weight sum.</returns>
    public static Matching MaximumWeightMatching(int leftCount, int rightCount, List<List<(int vertex, int weight)>> adjacencyList)
    {
        int L = leftCount;
        int R = rightCount;
        
        int[] leftPairs = new int[L];
        int[] rightPairs = new int[R];
        int[] rightParents = new int[R];
        bool[] rightVisited = new bool[R];
        
#if NET8_0_OR_GREATER
        Array.Fill(leftPairs, -1);
        Array.Fill(rightPairs, -1);
        Array.Fill(rightParents, -1);
#else
        for (int i = 0; i < L; i++) leftPairs[i] = -1;
        for (int i = 0; i < R; i++) rightPairs[i] = -1;
        for (int i = 0; i < R; i++) rightParents[i] = -1;
#endif
        
        List<int> visitedLefts = new List<int>(L);
        List<int> visitedRights = new List<int>(R);
        List<int> onEdgeRights = new List<int>(R);
        bool[] rightOnEdge = new bool[R];
        
        int[] leftLabels = new int[L];
        int[] rightLabels = new int[R];
        int[] slacks = new int[R];
       
        List<List<(int vertex, int weight)>> optimizedAdj = new List<List<(int vertex, int weight)>>(L);
        for (int l = 0; l < L; l++)
        {
            List<(int vertex, int weight)> edges = adjacencyList[l];
            
            int maxWeight = 0;
            for (int i = 0; i < edges.Count; i++)
            {
                if (edges[i].weight > maxWeight)
                    maxWeight = edges[i].weight;
            }
            leftLabels[l] = maxWeight;
            optimizedAdj.Add(RetainTopK(edges, L));
        }
 
#if NET8_0_OR_GREATER
        Array.Fill(slacks, int.MaxValue);
#else
        for (int i = 0; i < R; i++)
        {
            slacks[i] = int.MaxValue;
        }        
#endif

        Queue<int> q = new Queue<int>();
        
        for (int l = 0; l < L; l++)
        {
            foreach ((int r, int w) in optimizedAdj[l])
            {
                if (rightPairs[r] == -1 && leftLabels[l] + rightLabels[r] == w)
                {
                    leftPairs[l] = r;
                    rightPairs[r] = l;
                    break;
                }
            }
        }

        for (int l = 0; l < L; l++)
        {
            if (leftPairs[l] != -1) continue;
            
            q.Clear();
         
            foreach (int r in visitedRights)
                rightVisited[r] = false;
            
            foreach (int r in onEdgeRights)
            {
                rightOnEdge[r] = false;
                slacks[r] = int.MaxValue;
            }
            
            visitedLefts.Clear();
            visitedRights.Clear();
            onEdgeRights.Clear();
            
            visitedLefts.Add(l);
            q.Enqueue(l);
            
            int firstUnmatchedR = -1;
            for (int r = 0; r < R; r++)
            {
                if (rightPairs[r] == -1)
                {
                    firstUnmatchedR = r;
                    break;
                }
            }
          
            if (firstUnmatchedR != -1)
            {
                BfsUntilAppliesAugmentPath(firstUnmatchedR);
            }
        }
        
        int sum = 0;
        for (int l = 0; l < L; l++)
        {
            if (leftPairs[l] != -1)
            {
                int r = leftPairs[l];
                sum += leftLabels[l] + rightLabels[r];
            }
        }
        
        return new Matching(leftPairs, rightPairs, sum);
        
        bool Advance(int r)
        {
            rightOnEdge[r] = false;
            rightVisited[r] = true;
            visitedRights.Add(r);
            int l = rightPairs[r];
            
            if (l != -1)
            {
                q.Enqueue(l);
                visitedLefts.Add(l);
                return false;
            }
            
            while (r != -1)
            {
                l = rightParents[r];
                int prevR = leftPairs[l];
                leftPairs[l] = r;
                rightPairs[r] = l;
                r = prevR;
            }
            return true;
        }
        
        void BfsUntilAppliesAugmentPath(int firstUnmatchedR)
        {
            while (true)
            {
                while (q.Count > 0)
                {
                    int l = q.Dequeue();
                    
                    if (leftLabels[l] == 0)
                    {
                        rightParents[firstUnmatchedR] = l;
                        if (Advance(firstUnmatchedR))
                            return;
                    }
                    
                    if (slacks[firstUnmatchedR] > leftLabels[l])
                    {
                        slacks[firstUnmatchedR] = leftLabels[l];
                        rightParents[firstUnmatchedR] = l;
                        if (!rightOnEdge[firstUnmatchedR])
                        {
                            onEdgeRights.Add(firstUnmatchedR);
                            rightOnEdge[firstUnmatchedR] = true;
                        }
                    }
                    
                    foreach ((int r, int w) in optimizedAdj[l])
                    {
                        if (rightVisited[r]) continue;
                        
                        int diff = leftLabels[l] + rightLabels[r] - w;
                        
                        if (diff == 0)
                        {
                            rightParents[r] = l;
                            if (Advance(r)) return;
                        }
                        else if (slacks[r] > diff)
                        {
                            rightParents[r] = l;
                            slacks[r] = diff;
                            if (!rightOnEdge[r])
                            {
                                onEdgeRights.Add(r);
                                rightOnEdge[r] = true;
                            }
                        }
                    }
                }
                
                int delta = int.MaxValue;
                foreach (int r in onEdgeRights)
                {
                    if (rightOnEdge[r])
                        delta = Math.Min(delta, slacks[r]);
                }
                
                foreach (int l in visitedLefts)
                    leftLabels[l] -= delta;
                
                foreach (int r in visitedRights)
                    rightLabels[r] += delta;
                
                foreach (int r in onEdgeRights)
                {
                    if (rightOnEdge[r])
                    {
                        slacks[r] -= delta;
                        if (slacks[r] == 0 && Advance(r))
                            return;
                    }
                }
            }
        }
    }
    
#if NET8_0_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
#endif
   static (int[] leftPairs, int[] rightPairs, int weightSum) MaximumWeightMatching(
        int leftCount, int rightCount,
        int[] edgeVertices, int[] edgeWeights, int[] rowOffsets, int[] leftLabels)
    {
        int[] leftPairs = new int[leftCount];
        int[] rightPairs = new int[rightCount];
        int[] rightParents = new int[rightCount];
        int[] rightLabels = new int[rightCount];
        int[] slacks = new int[rightCount];
        bool[] rightVisited = new bool[rightCount];
        bool[] rightOnEdge = new bool[rightCount];
        
#if NET8_0_OR_GREATER
        Array.Fill(leftPairs, -1);
        Array.Fill(rightPairs, -1);
        Array.Fill(rightParents, -1);
#else
        for (int i = 0; i < leftCount; i++) leftPairs[i] = -1;
        for (int i = 0; i < rightCount; i++) rightPairs[i] = -1;
        for (int i = 0; i < rightCount; i++) rightParents[i] = -1;
#endif
        
#if NET8_0_OR_GREATER
        Array.Fill(slacks, int.MaxValue);
#else
        for (int i = 0; i < rightCount; i++)
            slacks[i] = int.MaxValue;
#endif
        
        int[] visitedLeftsBuffer = new int[leftCount];
        int[] visitedRightsBuffer = new int[rightCount];
        int[] onEdgeRightsBuffer = new int[rightCount];
        
        int visitedLeftsCount;
        int visitedRightsCount = 0;
        int onEdgeRightsCount = 0;
        int activeOnEdgeCount;
        int minSlack;
        int[] queueBuffer = new int[leftCount];
        int qHead;
        int qTail;
        
        for (int l = 0; l < leftCount; l++)
        {
            int start = rowOffsets[l];
            int end = rowOffsets[l + 1];
            for (int i = start; i < end; i++)
            {
                int r = edgeVertices[i];
                int w = edgeWeights[i];
                if (rightPairs[r] == -1 && leftLabels[l] + rightLabels[r] == w)
                {
                    leftPairs[l] = r;
                    rightPairs[r] = l;
                    break;
                }
            }
        }
        
        int firstUnmatchedR = 0;

        for (int l = 0; l < leftCount; l++)
        {
            if (leftPairs[l] != -1) continue;
            
            qHead = 0;
            qTail = 0;
            
            for (int i = 0; i < visitedRightsCount; i++)
                rightVisited[visitedRightsBuffer[i]] = false;
            
            for (int i = 0; i < onEdgeRightsCount; i++)
            {
                int r = onEdgeRightsBuffer[i];
                rightOnEdge[r] = false;
                slacks[r] = int.MaxValue;
            }
            
            visitedLeftsCount = 0;
            visitedRightsCount = 0;
            onEdgeRightsCount = 0;
            activeOnEdgeCount = 0;
            minSlack = int.MaxValue;
            
            visitedLeftsBuffer[visitedLeftsCount++] = l;
            queueBuffer[qTail++] = l;
            
            while (firstUnmatchedR < rightCount && rightPairs[firstUnmatchedR] != -1)
                firstUnmatchedR++;
            
            if (firstUnmatchedR < rightCount)
            {
                BfsUntilAppliesAugmentPath(firstUnmatchedR);
            }
        }
        
        int sum = 0;
        for (int l = 0; l < leftCount; l++)
        {
            if (leftPairs[l] != -1)
            {
                int r = leftPairs[l];
                sum += leftLabels[l] + rightLabels[r];
            }
        }
        
        return (leftPairs, rightPairs, sum);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool Advance(int r)
        {
            if (rightOnEdge[r])
            {
                rightOnEdge[r] = false;
                activeOnEdgeCount--;
            }
            rightVisited[r] = true;
            visitedRightsBuffer[visitedRightsCount++] = r;
            int l = rightPairs[r];
            
            if (l != -1)
            {
                queueBuffer[qTail++] = l;
                visitedLeftsBuffer[visitedLeftsCount++] = l;
                return false;
            }
            
            while (r != -1)
            {
                l = rightParents[r];
                int prevR = leftPairs[l];
                leftPairs[l] = r;
                rightPairs[r] = l;
                r = prevR;
            }
            return true;
        }

        void BfsUntilAppliesAugmentPath(int firstRWithoutMatch)
        {
            while (true)
            {
                while (qHead < qTail)
                {
                    int l = queueBuffer[qHead++];
                    
                    int labelL = leftLabels[l];
                    
                    if (labelL == 0)
                    {
                        rightParents[firstRWithoutMatch] = l;
                        if (Advance(firstRWithoutMatch))
                            return;
                    }
                    
                    if (slacks[firstRWithoutMatch] > labelL)
                    {
                        slacks[firstRWithoutMatch] = labelL;
                        minSlack = labelL < minSlack ? labelL : minSlack;
                        rightParents[firstRWithoutMatch] = l;
                        if (!rightOnEdge[firstRWithoutMatch])
                        {
                            onEdgeRightsBuffer[onEdgeRightsCount++] = firstRWithoutMatch;
                            rightOnEdge[firstRWithoutMatch] = true;
                            activeOnEdgeCount++;
                        }
                    }
                    
                    int start = rowOffsets[l];
                    int end = rowOffsets[l + 1];
                    
#if NET8_0_OR_GREATER
                    int edgeCount = end - start;
                    ReadOnlySpan<int> vertices = edgeVertices.AsSpan(start, edgeCount);
                    ReadOnlySpan<int> weights = edgeWeights.AsSpan(start, edgeCount);
                    
                    for (int i = 0; i < vertices.Length; i++)
                    {
                        int r = vertices[i];
                        int w = weights[i];
#else
                    for (int i = start; i < end; i++)
                    {
                        int r = edgeVertices[i];
                        int w = edgeWeights[i];
#endif
                        
                        if (rightVisited[r]) continue;
                        
                        int diff = labelL + rightLabels[r] - w;
                        
                        if (diff != 0)
                        {
                            if (slacks[r] > diff)
                            {
                                rightParents[r] = l;
                                int oldSlack = slacks[r];
                                slacks[r] = diff;
                                minSlack = diff < minSlack ? diff : minSlack;
                                
                                if (minSlack == 0) break;
                                
                                if (oldSlack == int.MaxValue)
                                {
                                    onEdgeRightsBuffer[onEdgeRightsCount++] = r;
                                    rightOnEdge[r] = true;
                                    activeOnEdgeCount++;
                                }
                            }
                        }
                        else
                        {
                            rightParents[r] = l;
                            if (Advance(r)) return;
                        }
                    }
                }
                
                int delta = minSlack;
 
                for (int i = 0; i < visitedLeftsCount; i++)
                    leftLabels[visitedLeftsBuffer[i]] -= delta;
                
                for (int i = 0; i < visitedRightsCount; i++)
                    rightLabels[visitedRightsBuffer[i]] += delta;
                
                minSlack = int.MaxValue;
                for (int i = 0; i < onEdgeRightsCount && activeOnEdgeCount > 0; i++)
                {
                    int r = onEdgeRightsBuffer[i];
                    if (rightOnEdge[r])
                    {
                        slacks[r] -= delta;
                        if (slacks[r] == 0)
                        {
                            if (Advance(r))
                                return;
                        }
                        else
                        {
                            minSlack = slacks[r] < minSlack ? slacks[r] : minSlack;
                        }
                    }
                }
            }
        }
    }
   
    private static void RetainTopKInPlace((int vertex, int weight)[] edges, int edgeCount, int k)
    {
        if (edgeCount <= k)
            return;
        
        QuickSelectTopK(edges, 0, edgeCount - 1, k);
    }
    
    private static void QuickSelectTopK((int vertex, int weight)[] edges, int left, int right, int k)
    {
        while (left < right)
        {
            int pivotIndex = MedianOfThree(edges, left, right);
            pivotIndex = Partition(edges, left, right, pivotIndex);
            
            if (pivotIndex == k - 1)
                return;
            if (pivotIndex < k - 1)
                left = pivotIndex + 1;
            else
                right = pivotIndex - 1;
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int Partition((int vertex, int weight)[] edges, int left, int right, int pivotIndex)
    {
        int pivotWeight = edges[pivotIndex].weight;
        (edges[pivotIndex], edges[right]) = (edges[right], edges[pivotIndex]);
        
        int storeIndex = left;
        for (int i = left; i < right; i++)
        {
            if (edges[i].weight > pivotWeight)
            {
                (edges[i], edges[storeIndex]) = (edges[storeIndex], edges[i]);
                storeIndex++;
            }
        }
        
        (edges[storeIndex], edges[right]) = (edges[right], edges[storeIndex]);
        return storeIndex;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int MedianOfThree((int vertex, int weight)[] edges, int left, int right)
    {
        int mid = left + (right - left) / 2;
        int leftWeight = edges[left].weight;
        int midWeight = edges[mid].weight;
        int rightWeight = edges[right].weight;
        
        if (leftWeight >= midWeight != leftWeight >= rightWeight)
            return left;
        return midWeight >= leftWeight != midWeight >= rightWeight ? mid : right;
    }
    
    /// <summary>
    /// Finds the optimal assignments for a given matrix of agents and costed tasks such that the total cost is minimized using modified Kwok's algorithm.
    /// </summary>
    /// <param name="costs">A cost matrix; the element at row <em>i</em> and column <em>j</em> represents the cost of agent <em>i</em> performing task <em>j</em>.</param>
    /// <returns>A matrix of assignments; the value of element <em>i</em> is the column of the task assigned to agent <em>i</em>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="costs"/> is null.</exception>
    public static int[] FindAssignments(int[,] costs)
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(nameof(costs));
#else
        if (costs == null)
            throw new ArgumentNullException(nameof(costs));
#endif

        Matching result = FindAssignmentsWithWeight(costs);
        return result.LeftPairs;
    }
    
    /// <summary>
    /// Finds the optimal assignments for a given matrix of agents and costed tasks such that the total cost is minimized using modified Kwok's algorithm.
    /// </summary>
    /// <param name="costs">A cost matrix; the element at row <em>i</em> and column <em>j</em> represents the cost of agent <em>i</em> performing task <em>j</em>.</param>
    /// <returns>A full matching.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="costs"/> is null.</exception>
    public static Matching FindAssignmentsWithWeight(int[,] costs)
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(nameof(costs));
#else
        if (costs == null)
            throw new ArgumentNullException(nameof(costs));
#endif
        
        int h = costs.GetLength(0);
        int w = costs.GetLength(1);
        bool transposed = h > w;
        
        if (transposed)
        {
            int[,] temp = new int[w, h];
            for (int i = 0; i < w; i++)
                for (int j = 0; j < h; j++)
                    temp[i, j] = costs[j, i];
            costs = temp;
            h = w;
            w = costs.GetLength(1);
        }
        
        int maxCost = 0;
        for (int i = 0; i < h; i++)
            for (int j = 0; j < w; j++)
                maxCost = Math.Max(maxCost, costs[i, j]);
        
        int maxEdgesPerRow = Math.Min(w, h); // Edge retention limit
        int totalEdges = h * maxEdgesPerRow;
        
        int[] edgeVertices = new int[totalEdges];
        int[] edgeWeights = new int[totalEdges];
        int[] rowOffsets = new int[h + 1];
        int[] leftLabels = new int[h];
        
        (int vertex, int weight)[] tempEdges = new (int vertex, int weight)[w];
        int flatIdx = 0;
        rowOffsets[0] = 0;
        
        for (int i = 0; i < h; i++)
        {
            int maxWeight = 0;
            
            for (int j = 0; j < w; j++)
            {
                int weight = maxCost + 1 - costs[i, j];
                tempEdges[j] = (j, weight);
                if (weight > maxWeight)
                    maxWeight = weight;
            }
            
            leftLabels[i] = maxWeight;
            
            RetainTopKInPlace(tempEdges, w, maxEdgesPerRow);
            int retainCount = Math.Min(w, maxEdgesPerRow);
  
            for (int j = 0; j < retainCount; j++)
            {
                edgeVertices[flatIdx] = tempEdges[j].vertex;
                edgeWeights[flatIdx] = tempEdges[j].weight;
                flatIdx++;
            }
            
            rowOffsets[i + 1] = flatIdx;
        }
        
        (int[] leftPairs, int[] rightPairs, int weightSum) = MaximumWeightMatching(h, w, edgeVertices, edgeWeights, rowOffsets, leftLabels);
   
        int actualCost = 0;
        for (int i = 0; i < leftPairs.Length; i++)
        {
            int j = leftPairs[i];
            if (j != -1)
                actualCost += costs[i, j];
        }
        
        if (transposed)
        {
            int[] originalAssignments = new int[w];
            for (int i = 0; i < w; i++)
                originalAssignments[i] = -1;
            
            for (int newRow = 0; newRow < leftPairs.Length; newRow++)
            {
                int newCol = leftPairs[newRow];
                if (newCol != -1)
                    originalAssignments[newCol] = newRow;
            }
            
            return new Matching(originalAssignments, [], actualCost);
        }
        
        return new Matching(leftPairs, rightPairs, actualCost);
    }
}